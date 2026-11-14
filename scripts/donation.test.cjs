const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const source = fs.readFileSync(path.join(__dirname, '../frontend/utils/donation.js'), 'utf8');
const modulePromise = import('data:text/javascript;base64,' + Buffer.from(source).toString('base64'));

test('accepts comma and dot decimals without floating-point total errors', async () => {
    const { donationEstimate } = await modulePromise;
    assert.deepEqual(donationEstimate('10,50', true, 5), { extra: 10.5, total: 15.5, error: '' });
    assert.deepEqual(donationEstimate('0.20', true, 0.1), { extra: 0.2, total: 0.3, error: '' });
    assert.equal(donationEstimate(',50', true, 5).extra, 0.5);
    assert.equal(donationEstimate('10,', true, 5).extra, 10);
});

test('invalid, empty and non-finite input cannot poison the total', async () => {
    const { donationEstimate } = await modulePromise;
    for (const raw of ['', null, 'abc', '-10', '0', '0,00', 'Infinity', '1e309', '10,1,2', '1.234,56', '10.123', '9'.repeat(300)]) {
        const result = donationEstimate(raw, true, 5);
        assert.ok(result.error, String(raw));
        assert.equal(result.extra, 0);
        assert.equal(result.total, 5);
        assert.ok(Number.isFinite(result.total));
    }
});

test('rejects amounts beyond safe cent precision including total overflow', async () => {
    const { donationEstimate } = await modulePromise;
    assert.ok(donationEstimate('90071992547409.92', true, 5).error);
    assert.ok(donationEstimate('90071992547409.90', true, 5).error);
});

test('turning off donation ignores an unfinished or invalid value', async () => {
    const { donationEstimate } = await modulePromise;
    assert.deepEqual(donationEstimate('invalid', false, 5), { extra: 0, total: 5, error: '' });
});

test('limits the full amount to ten digits, excluding the decimal separator', async () => {
    const { donationEstimate, donationDigitCount } = await modulePromise;
    assert.equal(donationDigitCount('12345678,90'), 10);
    assert.equal(donationEstimate('12345678,90', true, 5).error, '');
    assert.equal(donationEstimate('9999999999', true, 5).error, '');
    for (const value of ['12345678901', '123456789,01', '123456789.01']) {
        const result = donationEstimate(value, true, 5);
        assert.match(result.error, /10 cifre/);
        assert.equal(result.extra, 0);
        assert.equal(result.total, 5);
    }
});
