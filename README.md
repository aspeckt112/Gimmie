# Gimmie

## Generate a password

```sh
gimmie password
gimmie password --length 32
gimmie password -l 16 --no-symbols
gimmie password -n -s
gimmie password --copy
```

`password` prints one password followed by a newline. It uses cryptographically
secure, unbiased random selection and includes at least one character from every
enabled class. Ambiguous characters (all cases of I/L/O and digits 0/1) are omitted.

| Option | Short | Behavior |
| --- | --- | --- |
| `--length <count>` | `-l` | Default: 20; practical maximum: 1,024 characters |
| `--no-numbers` | `-n` | Exclude digits |
| `--no-symbols` | `-s` | Exclude punctuation |
| `--copy` | | Explicitly copy the password to the clipboard |

Lowercase and uppercase are always enabled. Length must be positive and large
enough to include every enabled class: at least 4 normally, 3 with either exclusion,
or 2 with both. Invalid lengths are rejected with a clear error and nonzero exit
code; no password is generated or copied.

The exact alphabets are:

| Class | Characters |
| --- | --- |
| Lowercase | `abcdefghjkmnpqrstuvwxyz` |
| Uppercase | `ABCDEFGHJKMNPQRSTUVWXYZ` |
| Digits | `23456789` |

Symbols: `!#$%&()*+,-./:;<=>?@[]^_{|}~`

Symbols are printable ASCII punctuation excluding whitespace, single and double
quotes, backslash, and backtick.

Passwords are never logged or stored by Gimmie. Copying is **off by default**;
global `--copy` also works before the command and copies without the display
newline. Clipboard failures return exit code 1 with a fixed diagnostic; the
password remains on stdout.

Terminal output, redirected stdout, and clipboard history may retain secrets
outside Gimmie's control. Use those destinations deliberately.

Run `gimmie password --help` for command help.
