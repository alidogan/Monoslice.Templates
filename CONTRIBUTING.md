# Contributing

Thanks for helping to improve Monoslice!

## Layout

- `templates/solution` – the `ms-sln` template. It is a normal, buildable solution: open it, build it and run its
  tests like any other project. Template options are expressed with `#if (Symbol)` blocks; during template
  development the default variant is compiled (see the `TemplateDevOnly` block in `Directory.Build.props`).
  Build another variant with, for example:

  ```bash
  cd templates/solution
  dotnet build -p:TemplateDatabase=SqlServer -p:TemplateBroker=RabbitMq -p:TemplateAuth=none
  ```

- `templates/module` – the `ms-module` item template.
- `templates/feature` – the `ms-feature` item template.
- `Monoslice.Templates.csproj` – packs everything into the `Monoslice.Templates` NuGet package.

## Checking your change

Docker must be running (the generated solutions use Testcontainers).

```bash
./scripts/test-templates.sh            # packs, installs and tests the most important variants
./scripts/test-templates.sh --all      # every variant
```

When the sample Catalog model changes, regenerate its migrations for both providers:

```bash
./scripts/regenerate-migrations.sh
```

## Pull requests

- Keep the generated code idiomatic and small; every line ends up in someone's product.
- Add or update tests in the generated solution for behavior you add.
- Add an entry to `CHANGELOG.md`.
