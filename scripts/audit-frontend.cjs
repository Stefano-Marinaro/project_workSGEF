// Browser-free checks for frontend syntax, literal routes, API paths and icon names.
const fs = require('node:fs');
const path = require('node:path');
const { createRequire } = require('node:module');
const root = path.resolve(__dirname, '..');
const front = path.join(root, 'frontend');
const fromFrontend = createRequire(path.join(front, 'package.json'));
const { parse } = fromFrontend('@babel/parser');
const traverse = fromFrontend('@babel/traverse').default;
const globals = new Set([...Object.getOwnPropertyNames(globalThis), 'window', 'document', 'localStorage', 'navigator', 'alert', '__DEV__', 'require']);
const glyphRoot = path.join(front, 'node_modules/@expo/vector-icons/build/vendor/react-native-vector-icons/glyphmaps');
const ionicons = JSON.parse(fs.readFileSync(path.join(glyphRoot, 'Ionicons.json'), 'utf8'));
const material = JSON.parse(fs.readFileSync(path.join(glyphRoot, 'MaterialCommunityIcons.json'), 'utf8'));
const files = [];
function collect(directory) {
  for (const entry of fs.readdirSync(directory, { withFileTypes: true })) {
    if (['node_modules', '.expo', 'dist', 'scripts'].includes(entry.name)) continue;
    const filename = path.join(directory, entry.name);
    if (entry.isDirectory()) collect(filename);
    else if (/\.[cm]?[jt]sx?$/.test(filename)) files.push(filename);
  }
}
collect(front);
const routes = files.filter(file => file.startsWith(path.join(front, 'app') + path.sep) && !path.basename(file).startsWith('_'))
  .map(file => '/' + path.relative(path.join(front, 'app'), file).replace(/\\/g, '/').replace(/\.[jt]sx?$/, '')
    .split('/').filter(part => !/^\(.+\)$/.test(part)).join('/').replace(/\/?index$/, ''));
const normalize = value => value.replace(/\([^/]+\)\//g, '').split('?')[0].replace(/\/$/, '') || '/';
const routeMatch = (target, actual) => {
  const parts = normalize(target).split('/');
  const pattern = normalize(actual).split('/');
  return parts.length === pattern.length && parts.every((part, i) => /^\[.+\]$/.test(pattern[i]) || part === pattern[i]);
};
const calls = [];
const problems = [];
let icons = 0, links = 0;
function literal(node) {
  if (node?.type === 'StringLiteral') return node.value;
  if (node?.type === 'TemplateLiteral') return node.quasis.map(part => part.value.cooked).join('{id}');
  return null;
}
function walk(node, file) {
  if (!node || typeof node !== 'object') return;
  const location = `${path.relative(root, file)}:${node.loc?.start.line || 1}`;
  if (node.type === 'CallExpression' && node.callee.type === 'MemberExpression') {
    const object = node.callee.object.name;
    const method = node.callee.property.name;
    if (object === 'api' && ['get', 'post', 'put', 'patch', 'delete'].includes(method)) {
      const value = literal(node.arguments[0]);
      if (value) calls.push({ method, value, location });
    }
    if (object === 'router' && ['push', 'replace', 'navigate', 'dismissTo'].includes(method)) {
      const argument = node.arguments[0];
      const target = literal(argument?.type === 'ObjectExpression'
        ? argument.properties.find(property => property.key?.name === 'pathname')?.value : argument);
      if (target && !target.includes('{id}') && target.startsWith('/')) {
        links++;
        if (!routes.some(route => routeMatch(target, route))) problems.push(`${location}: unknown app route ${target}`);
      }
    }
  }
  if (node.type === 'JSXOpeningElement' && ['Icon', 'Ionicons', 'MaterialCommunityIcons'].includes(node.name.name)) {
    const value = node.attributes.find(attribute => attribute.name?.name === 'name')?.value;
    const name = literal(value?.type === 'JSXExpressionContainer' ? value.expression : value);
    if (name && !name.includes('{id}')) {
      icons++;
      const isMaterial = name.startsWith('mc:') || node.name.name === 'MaterialCommunityIcons';
      const key = name.replace(/^mc:/, '');
      if (!(key in (isMaterial ? material : ionicons))) problems.push(`${location}: unknown icon ${name}`);
    }
  }
  for (const child of Object.values(node)) {
    if (Array.isArray(child)) child.forEach(value => walk(value, file));
    else if (child && typeof child === 'object') walk(child, file);
  }
}
(async () => {
  for (const file of files) {
    try {
      const ast = parse(fs.readFileSync(file, 'utf8'), { sourceType: 'unambiguous', plugins: ['jsx', 'typescript'] });
      walk(ast, file);
      traverse(ast, { ReferencedIdentifier(reference) {
        const name = reference.node.name;
        if (!reference.scope.hasBinding(name) && !globals.has(name)) {
          problems.push(`${path.relative(root, file)}:${reference.node.loc.start.line}: undefined identifier ${name}`);
        }
      } });
    }
    catch (error) { problems.push(`${path.relative(root, file)}: ${error.message}`); }
  }
  const response = await fetch((process.env.AUDIT_API_URL || 'http://localhost:5161') + '/swagger/v1/swagger.json');
  if (!response.ok) throw new Error(`API schema returned ${response.status}`);
  const schema = await response.json();
  const apiNormalize = value => value.replace(/\{[^}]+\}/g, '{id}').split('?')[0];
  for (const call of calls) {
    const match = Object.entries(schema.paths).some(([route, operations]) =>
      apiNormalize(call.value) === apiNormalize(route) && call.method in operations);
    if (!match) problems.push(`${call.location}: API contract mismatch ${call.method.toUpperCase()} ${call.value}`);
  }
  const compose = fs.readFileSync(path.join(root, 'docker-compose.yml'), 'utf8');
  for (const match of compose.matchAll(/Frontend__\w+:\s*"\$\{[^}]+\}([^"?]+)"/g)) {
    const target = match[1];
    if (!routes.some(route => routeMatch(target, route) || routeMatch(target + '/token', route))) {
      problems.push(`docker-compose.yml: backend-generated link has no app page: ${target}`);
    }
  }
  console.log(`Checked ${files.length} source files, ${links} literal navigation targets, ${icons} icon names and ${calls.length} API calls.`);
  problems.forEach(problem => console.log('FAIL ' + problem));
  console.log(`${problems.length} problems found.`);
  process.exitCode = problems.length ? 1 : 0;
})().catch(error => { console.error(error.message); process.exitCode = 1; });
