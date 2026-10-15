"""Dependency-free validator for the JSON Schema subset used by this project.

Supports type, required, additionalProperties, properties, items, enum and
numeric bounds. These schema files are also standard Draft 2020-12 documents.
Run from any directory: python Tools/validate_game_data.py
"""
from pathlib import Path
import json
import math

ROOT = Path(__file__).resolve().parents[1]
DATA = ROOT / 'Assets/Resources/Data'


def unique_object(pairs):
    result = {}
    for key, value in pairs:
        if key in result:
            raise ValueError(f'duplicate JSON key: {key}')
        result[key] = value
    return result


def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'), object_pairs_hook=unique_object)


def validate(value, schema, path):
    kind = schema.get('type')
    valid = {
        'object': isinstance(value, dict), 'array': isinstance(value, list),
        'string': isinstance(value, str), 'boolean': isinstance(value, bool),
        'null': value is None,
        'integer': type(value) is int,
        'number': type(value) in (int, float),
        None: True,
    }[kind]
    assert valid, f'{path}: expected {kind}'
    if type(value) in (int, float):
        assert math.isfinite(value), f'{path}: non-finite number'
        assert value >= schema.get('minimum', -math.inf), f'{path}: below minimum'
        assert value <= schema.get('maximum', math.inf), f'{path}: above maximum'
    if 'enum' in schema:
        assert value in schema['enum'], f'{path}: invalid enum'
    if isinstance(value, dict):
        assert set(schema.get('required', [])) <= value.keys(), f'{path}: missing field'
        properties = schema.get('properties', {})
        if schema.get('additionalProperties') is False:
            assert value.keys() <= properties.keys(), f'{path}: unknown field'
        for key, child in value.items():
            validate(child, properties.get(key, {}), f'{path}.{key}')
    if isinstance(value, list):
        for index, child in enumerate(value):
            validate(child, schema.get('items', {}), f'{path}[{index}]')


def main():
    manifest = read(DATA / 'manifest.json')
    files = list(DATA.rglob('*.json'))
    for path in files:
        assert path.stat().st_size <= manifest['maxFileBytes'], f'oversized file: {path}'
        read(path)
    index = read(DATA / 'Schemas/index.json')['documents']
    for relative, schema in index.items():
        validate(read(DATA / (relative + '.json')), read(DATA / (schema + '.json')), relative)
    for constraint in manifest['schemas']:
        item = read(DATA / (constraint + '.json'))
        fields = read(DATA / ('Schemas/' + item['group'] + '.schema.json'))['properties'][item['group']]['properties']
        assert set(fields) == {r['name'] for r in item['fields']}, 'schema/constraint mismatch'
        for rule in item['fields']:
            field = fields[rule['name']]
            assert rule['minimum'] == field['minimum'] and rule['maximum'] == field['maximum']
            assert rule['integer'] == (field['type'] == 'integer')
    assert not (DATA / 'catalog.json').exists(), 'obsolete monolithic catalog remains'
    print(f'PASS: {len(files)} JSON files; {len(index)} schema-validated documents; '
          f'max {max(p.stat().st_size for p in files)} bytes; constraints synchronized.')


if __name__ == '__main__':
    main()
