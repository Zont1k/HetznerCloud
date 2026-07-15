# Contributing to Hetzner Cloud .NET

Thank you for your interest in contributing! This document provides guidelines for contributing to the project.

## Code of Conduct

This project follows the [Contributor Covenant Code of Conduct](https://www.contributor-covenant.org/version/2/1/code_of_conduct/). By participating, you are expected to uphold this code.

## How to Contribute

### Reporting Bugs

Before creating a bug report, please check the [existing issues](https://github.com/hetznercloud/hcloud-dotnet/issues) to avoid duplicates.

When creating a bug report, include:
- Library version
- .NET version
- Steps to reproduce
- Expected vs actual behavior
- Relevant code snippets
- Error messages/stack traces

### Suggesting Enhancements

Enhancement suggestions are welcome! Please provide:
- Use case description
- Proposed API design (if applicable)
- Any breaking change considerations

### Pull Requests

1. **Fork** the repository
2. **Create a branch** from `main`: `git checkout -b feature/your-feature-name`
3. **Make changes** following the coding standards below
4. **Run tests**: `dotnet test`
5. **Commit** with conventional commit messages
6. **Push** and create a Pull Request

## Development Setup

### Prerequisites
- .NET 8.0 SDK or later
- Git

### Clone and Build
```bash
git clone https://github.com/hetznercloud/hcloud-dotnet.git
cd hcloud-dotnet
dotnet restore
dotnet build
```

### Run Tests
```bash
dotnet test
```

### Run Example
```bash
export HCLOUD_TOKEN=your_token
dotnet run --project examples/HetznerCloud.Example.csproj
```

## Coding Standards

### General
- Follow [.NET Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use `var` when type is obvious
- Prefer expression-bodied members for simple properties/methods
- Use nullable reference types (enabled in project)
- Avoid `async void` except for event handlers

### Naming
- **Classes/Interfaces/Enums**: PascalCase
- **Methods/Properties**: PascalCase
- **Parameters/Locals**: camelCase
- **Constants**: PascalCase
- **Private fields**: `_camelCase`
- **Interfaces**: Prefix with `I` (e.g., `IServerClient`)

### Documentation
- All public APIs must have XML documentation (`///`)
- Include `<summary>`, `<param>`, `<returns>`, `<exception>` where applicable
- Document nullable parameters and return values

### Git Commits
Follow [Conventional Commits](https://www.conventionalcommits.org/):
- `feat:` New feature
- `fix:` Bug fix
- `docs:` Documentation only
- `refactor:` Code restructuring
- `test:` Adding tests
- `chore:` Maintenance

Example: `feat: add support for server rescue mode`

### Pull Request Requirements
- All tests pass
- Code coverage maintained or improved
- No new warnings
- Updated documentation if API changed
- CHANGELOG.md updated (maintainers will handle versioning)

## API Design Guidelines

When adding new endpoints:

1. **Follow Hetzner API** - Match parameter names and behavior
2. **Strong Typing** - Use enums for fixed values, not strings
3. **Async Only** - All I/O methods must be async
4. **Cancellation** - Accept `CancellationToken` parameter
5. **Validation** - Validate required parameters early
6. **Errors** - Throw specific exceptions, not generic `Exception`
6. **Pagination** - Use `PaginationRequest` for list operations
7. **Actions** - Return `ActionResponse` for async operations, provide `WaitForAsync` helpers

## Testing

### Unit Tests
- Mock `HttpMessageHandler` for HTTP calls
- Test serialization/deserialization
- Test parameter validation
- Test exception mapping

### Integration Tests
- Require `HCLOUD_TOKEN` environment variable
- Mark with `[Trait("Category", "Integration")]`
- Run separately: `dotnet test --filter "Category=Integration"`

## Releasing

Releases are automated via GitHub Actions:
1. Maintainer creates a release tag: `git tag v1.0.0 && git push origin v1.0.0`
2. CI builds, tests, packs, and publishes to NuGet
3. GitHub Release created with changelog

## Questions?

- Open a [Discussion](https://github.com/hetznercloud/hcloud-dotnet/discussions)
- Check [Hetzner Cloud API Docs](https://docs.hetzner.cloud)
- Review [hcloud-go](https://github.com/hetznercloud/hcloud-go) for reference implementation

## License

By contributing, you agree that your contributions will be licensed under the MIT License.