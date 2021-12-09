const yaml = require('js-yaml');
const fs = require('fs');
const { exit } = require('process');

const input_path = process.argv[2]
const output_path = process.argv[3]

if (!input_path || !output_path) {
    console.error('No path')
    exit(1)
}

console.log(`Reading from '${input_path}' and writing to '${output_path}'`)

// Get document, or throw exception on error
try {
    const doc = yaml.load(fs.readFileSync(input_path, 'utf8'));
    fs.writeFileSync(output_path, JSON.stringify(doc), { encoding: 'UTF-8' })
} catch (e) {
    console.log(e);
}
