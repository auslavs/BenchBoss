# Repo-wide Agent Instructions

- This repository's GitHub Actions workflow (`.github/workflows/build-docs.yml`) is responsible for building the application and committing the output into `docs/` on pushes to `main`.
- Pull request builds must write their preview output to `docs/pr/<sanitized-branch>/` (matching the sanitisation used in the workflow) and publish a comment in the PR linking to the preview hosted on GitHub Pages.
- Do not remove or disable this automation. If you change the build process or Vite configuration, keep the workflow behaviour above intact.
