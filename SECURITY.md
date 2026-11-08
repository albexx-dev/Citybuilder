# Security Policy

## Supported version

| Version | Supported |
| --- | --- |
| v1.1 | yes |
| older prototype builds | no |

## Reporting a vulnerability

Please **do not** open a public issue for a security problem. Use GitHub's private "Report a vulnerability" flow (Security → Advisories) or contact the repository owner directly.

Include:

- affected version / commit;
- reproduction steps;
- impact and, if possible, a minimal fix suggestion.

## Secrets and configuration

- The repository intentionally contains **no** API keys, tokens, passwords or cloud credentials.
- Unity platform secret fields in `ProjectSettings/ProjectSettings.asset` are empty.
- WorkBuddy AI / Unity MCP configuration lives outside the repository; copy `Tools/mcp.example.json` and keep real credentials out of git.
- `Tools/ValidateRepo.py` fails CI if obvious secret patterns are committed.

## Dependencies

Unity packages are pinned in `Packages/manifest.json` and `Packages/packages-lock.json`. The external Unity MCP bridge is pinned to tag `v10.3.0` and commit `aa5fc638d623f56178d50329d9ad8c541a57fe66`. Review any dependency change in its own commit.
