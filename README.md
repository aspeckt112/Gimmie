# Gimmie

## Generate a random integer or choice

```sh
gimmie random integer
gimmie random integer --min -10 --max 10
gimmie random choice red green blue
printf 'red\ngreen\nblue\n' | gimmie random choice
gimmie random choice -- --copy
gimmie random integer --copy
```

`random integer` generates one integer using cryptographically secure, unbiased
randomness. Bounds are inclusive signed 32-bit integers; `--min` defaults to 0
and `--max` defaults to 100. Equal bounds return that value. Invalid bounds or
a minimum greater than the maximum produce an error and a nonzero exit code.

`random choice` selects one entry using the same secure, unbiased randomness.
Pass separate positional arguments or redirect newline-separated input. If any
positional arguments are supplied, they take precedence over stdin, even if all
are empty. Empty arguments and lines are ignored; whitespace in nonempty entries
is preserved, and repeated entries are separate choices. An empty list produces
an error and a nonzero exit code. Use `--` before entries that must not be parsed
as options, such as a literal `--copy`.

Both commands print the result followed by a display newline. Clipboard copying
is off by default; global `--copy` explicitly copies the result without the
display newline. Run `gimmie random integer --help` or
`gimmie random choice --help` for command help. Run `gimmie random --help`
to list the available random commands.

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
