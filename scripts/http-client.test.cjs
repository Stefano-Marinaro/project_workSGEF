const test = require('node:test');
const assert = require('node:assert/strict');
const vm = require('node:vm');
const fs = require('node:fs');
const path = require('node:path');

function session(refresh) {
  const store = { accessToken: 'old-access', refreshToken: 'old-refresh' };
  const redirects = [], retried = [];
  let onError, refreshCalls = 0;
  const api = async config => { retried.push(config.headers.Authorization); return { status: 200 }; };
  api.interceptors = { request: { use() {} }, response: { use(ok, error) { onError = error; } } };
  const source = fs.readFileSync(path.join(__dirname, '../frontend/config/httpClient.js'), 'utf8')
    .replace(/^import .*;\r?\n/gm, '').replace('export default api;', 'globalThis.api = api;');
  vm.runInNewContext(source, {
    axios: { create: () => api, post: async () => { refreshCalls++; return refresh(); } },
    router: { replace: value => redirects.push(value) }, API_BASE_URL: 'http://test',
    getItem: async key => store[key], setItem: async (key, value) => { store[key] = value; },
    deleteItem: async key => { delete store[key]; },
  });
  return {
    fail: () => onError({ response: { status: 401 }, config: { url: '/me/profile', headers: { Authorization: 'Bearer old-access' } } }),
    store, redirects, retried, refreshCalls: () => refreshCalls,
  };
}

test('parallel expired requests share one rotation and retry with the new token', async () => {
  let finish;
  const pending = new Promise(resolve => { finish = resolve; });
  const client = session(() => pending);
  const requests = [client.fail(), client.fail(), client.fail()];
  await new Promise(resolve => setImmediate(resolve));
  assert.equal(client.refreshCalls(), 1);
  finish({ data: { accessToken: 'new-access', refreshToken: 'new-refresh' } });
  await Promise.all(requests);
  assert.deepEqual(client.retried, Array(3).fill('Bearer new-access'));
  assert.equal(client.store.refreshToken, 'new-refresh');
  assert.equal(client.redirects.length, 0);
  await client.fail();
  assert.equal(client.refreshCalls(), 1, 'a late 401 reuses the token already refreshed');
});

test('temporary refresh failure preserves the session', async () => {
  const client = session(async () => { throw { response: { status: 503 } }; });
  await assert.rejects(client.fail());
  assert.equal(client.store.refreshToken, 'old-refresh');
  assert.equal(client.redirects.length, 0);
});

test('invalid refresh token clears the session and returns to login', async () => {
  const client = session(async () => { throw { response: { status: 403 } }; });
  await assert.rejects(client.fail());
  assert.equal(client.store.refreshToken, undefined);
  assert.deepEqual(client.redirects, ['/login']);
});
