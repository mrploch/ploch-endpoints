# Handover — establish `Ploch.Endpoints` as a standalone open-source library

| | |
|---|---|
| **Work item** | [PLO-776](https://linear.app/ploch/issue/PLO-776) (parent), in Linear project [ploch-endpoints](https://linear.app/ploch/project/ploch-endpoints-7625f0a64441) |
| **Predecessor** | [PLO-775](https://linear.app/ploch/issue/PLO-775): moved the code here from ploch-common |
| **Written** | 2026-10-13 by Claude Code session `3ace0022-f1fe-49bc-a139-9c175fb4a8f0` (run from `ploch-common`) |
| **Start the next session in** | `~/devnet/my/mrploch/ploch-endpoints` |
| **Source snapshot** | `mrploch/ploch-common` commit [`1f1764b`](https://github.com/mrploch/ploch-common/commit/1f1764b) |
| **Removal from ploch-common** | Pull request linked from [PLO-775](https://linear.app/ploch/issue/PLO-775) |

This document records everything the owner asked for. Read it in full before you start. Then follow
the workspace rules in `.claude/rules/`: Linear tracking, branch naming, commits, the PR completion
gate and the external AI review.

---

## 1. The owner's brief

These points restate the original request. Each one is a requirement.

1. The CRUD endpoints work started inside ploch-common, but it does not belong there.
   `ploch-endpoints` is its dedicated repository, and the library becomes **open source**.
2. **Rename everything.** The project name is **`Ploch.Endpoints`**, and every moved project gets a
   new name (see §4).
3. Create a **new solution** for the renamed projects.
4. Set up the **GitHub workflows exactly as in ploch-common and ploch-data**. That means build, test,
   code-coverage collection, static analysis, test-execution reporting and release workflows, plus
   whatever other features those repositories have (see §6).
5. Write a correct **README**.
6. Create a **sample app**.
7. Configure **NuGet packaging**, including a **README for each NuGet package**.
8. Generate **DocFX pages with the API documentation**, as the other open-source libraries do.
9. Set up the **GitHub repository** correctly, including **branch protection** (see §7).
10. If anything is unclear, **ask the owner**.

---

## 2. What is in this repository now

The code was copied verbatim at its original paths so that the rename can be traced. Nothing has
been renamed or fixed yet.

| Path (as imported) | Project | Purpose |
|---|---|---|
| `src/Common.WebApi/WebApi.Endpoints/` | `Ploch.Common.WebApi.Endpoints` | Core abstractions: `IEndpointHandler<TRequest,TResponse>`, `Models/` (`PaginatedRequest`, `PaginatedResponse<T>`, `IdRequest<TId>`, `DataTransferObjectRequest/Response<T>`, `EmptyResponse`) |
| `src/Common.WebApi/WebApi.Endpoints.CrudEndpoints/` | `Ploch.Common.WebApi.Endpoints.CrudEndpoints` | Framework-agnostic CRUD handlers (Create, GetById, GetList, Update, Delete) over `Ploch.Data.GenericRepository`, AutoMapper, `IMemoryCache`; the fluent DI builder `AddCrudEndpoints()` |
| `src/Common.WebApi/WebApi.Endpoints.CrudEndpoints.MinimalApiEndpoints/` | `...CrudEndpoints.MinimalApiEndpoints` | Minimal API route mapping (`CrudEndpointsMapper`) |
| `src/Common.WebApi/WebApi.Endpoints.CrudEndpoints.FastEndpoints/` | `...CrudEndpoints.FastEndpoints` | FastEndpoints endpoint classes that delegate to the handlers |
| `src/Common.WebApi/WebApi.Endpoints.CrudEndpoints.IntegrationTests/` | `...CrudEndpoints.IntegrationTests` | Integration tests (xUnit **v2**), currently under `src/` |
| `src/Common.WebApi/WebApi.Endpoints.Tests/` | `Ploch.Common.WebApi.Endpoints.Tests` | **Empty**: a csproj and no tests |
| `src/Common.WebApi/WebApi/` | `Ploch.Common.WebApi` | General ASP.NET Core helpers: `OpenApiConfigurator`, `QueryStringBinder` (`IsPackable=false`; never published) |
| `tests/Common.WebApi.Tests/` | `Ploch.Common.WebApi.Tests` | xUnit v3 tests for `OpenApiConfigurator` and `QueryStringBinder` |
| `src/Common.Ardalis.Result/` | `Ploch.Common.Ardalis.Result` | A single 22-line `ResultStatus.ToHttpStatusCode()` extension, used only by the FastEndpoints project. It was **never published to nuget.org**, so it moved with the code. |
| `docs/libraries/common-webapi.md` | — | The old documentation page from ploch-common, which describes the whole family. Use it as raw material for the new README and docs. |

**Not imported, on purpose:** the `bin/`/`obj/` folders, the untracked `*.csproj.user` files, and
ploch-common's two secondary solutions (`Ploch.Common.WebApi.Endpoints.slnx`,
`Ploch.Common.Endpoints.slnx`). Those solutions pulled in ploch-data projects by relative path and
are replaced by the new solution.

**Already present before the import** (the owner copied these in; they are uncommitted on `main`):

- `.agents/`, `.claude/`, `AGENTS.md`, `.github/agents/`, `.github/pull_request_template.md`,
  `.github/dependabot.yml`.
- `.github/workflows/{build-dotnet,code_quality,deploy-nuget-org,test_report}.yml`. **These are an
  old generation**: `actions/checkout@v3`, a SQL Server container, the `NUGET_APIKEY` secret and a
  separate `deploy-nuget-org.yml`. Replace them with the current ploch-common/ploch-data workflows
  (§6). Do not patch them.
- `AGENTS.md` points at `C:/DevNet/my/mrploch/ploch-endpoints/CLAUDE.md`, which is a Windows path,
  and **no `CLAUDE.md` exists yet**. Write one, modelled on `ploch-common/CLAUDE.md`, and fix the
  link.
- There is **no `LICENSE`** file. ploch-common and ploch-data both use **Apache-2.0**; confirm the
  licence with the owner.
- Review `.claude/settings.local.json` before committing anything under `.claude/`. It is
  machine-local, and the secret-protection rule applies.

---

## 3. Code review findings

A read-only review ran on 2026-10-13, and I verified the top findings against the source. Fix the
**must-fix** items before the first public package. Related Linear issues have moved into this
project: [PLO-322](https://linear.app/ploch/issue/PLO-322) (whether to publish),
[PLO-325](https://linear.app/ploch/issue/PLO-325) (`QueryStringBinder` and read-only properties),
[PLO-328](https://linear.app/ploch/issue/PLO-328) (GetById cache) and
[PLO-343](https://linear.app/ploch/issue/PLO-343) (about 90 hidden analyser warnings). Each of them
has a GitHub twin in **ploch-common**: #285, #288, #292 and #300. Those twins sit in another
repository, so **never** write `Closes #<n>` for them in a ploch-endpoints PR (see
`issue-tracking.md`). Close them by hand when the Linear issues finish.

### 3.1 Correctness: must fix

- **Create never persists.** `CrudEndpoints/Create/CreateEndpointHandler.cs:14-25` calls
  `repository.AddAsync` but never `unitOfWork.CommitAsync`. The returned DTO has no generated id,
  and nothing is saved. There is no Create test.
- **The GetById cache is never invalidated.** `CrudEndpoints/GetById/GetByIdEndpointHandler.cs:43-57`
  caches tracked entities for 5 minutes under `"{TypeName}_{id}"`. Update and Delete do not evict,
  the key uses `Name` rather than `FullName`, and a hit bypasses `GetByIdOperation`
  ([PLO-328](https://linear.app/ploch/issue/PLO-328)). `MapType` also registers a handler that needs
  `IMemoryCache` even when `WithMemoryCache()` was never called.
- **The paging bound check is wrong.** `CrudEndpoints/GetAll/GetAllIEndpointHandler.cs:38` computes
  `PageNumber * PageNumber > count`. It should be `(PageNumber - 1) * PageSize >= count`, and an
  empty first page should return an empty page, not 404. Add validation: `PageNumber >= 1`,
  `1 <= PageSize <= max`.
- **FastEndpoints verbs are swapped.** `FastEndpoints/CreateEndpoint.cs:16` uses `Put` and
  `UpdateEndpoint.cs:16` uses `Post`. Neither route has `{id}`. `Produces` metadata is wrong, and
  error results lose their message (`SendAsync(result.Value, …)`). Several calls drop the
  `CancellationToken`.
- **Minimal API routes are defective.** In `MinimalApiEndpoints/CrudEndpointsMapper.cs`:
  - `"/{id:int}"` is hard-coded, which breaks `Guid`/`string` ids.
  - The generic overloads re-prefix the base path, which gives `/blogposts/blogposts`.
  - The Create metadata says 204, but the handler returns 201.
  - `GetListEndpoints` binds `pageNumber`/`pageSize` as required, so omitting them gives 400
    instead of the defaults.
- **Update semantics.** The id comes only from the body, and a detached mapped entity
  full-replaces the stored one. `DataUpdateConcurrencyException` is not mapped to 409.
- **Delete disagrees with its test.** The handler returns `Result.NoContent()`, while
  `IntegrationTests/Delete/DeleteEndpointHandlerTests.cs:19` asserts `ResultStatus.Ok`.
- **The two flavours use different routes.** FastEndpoints uses `/api/{name}`; Minimal API uses
  `/{name}s`. Pluralisation is naive (`categorys`). Make the convention one configurable option.

### 3.2 Unfinished pieces

- Misnamed file: `GetAll/GetAllIEndpointHandler.cs` holds `GetListEndpointHandler`, and
  `GetListRequest` is unused.
- No FastEndpoints registration helper exists. The endpoints are open generics, which FastEndpoints
  does not discover.
- `EntityQueryOperations` is registered as a singleton keyed only by `TEntity`, so a second
  `MapType` for the same entity collides. `UpdateOperation` is dead code.
- XML docs are missing on almost every public type. The repository rule requires them, and
  `CS1591` must be clean ([PLO-343](https://linear.app/ploch/issue/PLO-343)).
- Test gaps: no Create tests, no HTTP-level tests for either flavour, and no cache or paging edge
  cases. The test DTOs live in the test project.

### 3.3 Project files that block a standalone build

- `WebApi.Endpoints.csproj` has a **Windows-only `HintPath`** to
  `Program Files\dotnet\shared\Microsoft.AspNetCore.App\9.0.0\Microsoft.AspNetCore.Mvc.Core.dll`.
  Replace it with `<FrameworkReference Include="Microsoft.AspNetCore.App" />`, or drop it.
- Sibling `ProjectReference`s must become NuGet `PackageReference`s. The latest nuget.org versions,
  checked 2026-10-13:

  | Current reference | Replace with |
  |---|---|
  | `../ploch-data/.../Ploch.Data.GenericRepository.csproj` | `Ploch.Data.GenericRepository` (4.0.1) |
  | `../ploch-data/.../Ploch.Data.GenericRepository.EFCore.csproj` | `Ploch.Data.GenericRepository.EFCore` (4.0.1) |
  | `../ploch-data/.../Ploch.Data.GenericRepository.EFCore.IntegrationTesting.csproj` (tests) | `Ploch.Data.GenericRepository.EFCore.IntegrationTesting` (4.0.1). The tests also use model types from ploch-data's *test* project; give this repository its own test model. |
  | `../../Common/Ploch.Common.csproj` | `Ploch.Common` (4.0.47) |
  | `../../Common.Ardalis.Result/...` | Fold `ToHttpStatusCode` into this repository (§4). |

- Delete the per-project `PackageVersion Update` / `PackageReference Update` blocks for Roslynator,
  SonarAnalyzer and Swashbuckle. They are IDE leftovers, and they **downgrade** the central analyser
  versions.
- `Microsoft.NET.Sdk.Web` is used for two class libraries (FastEndpoints, Minimal API). Use
  `Microsoft.NET.Sdk` plus the `Microsoft.AspNetCore.App` framework reference.
- Delete `Properties/launchSettings.json` and `appsettings*.json` from the library projects, and
  make sure `*.user` is ignored by git.
- Mixed test stacks: the integration tests use xUnit v2 with `Objectivity.AutoFixture.XUnit2.AutoMoq`,
  and the empty project uses xUnit v2. The workspace standard is **xUnit v3**, FluentAssertions and
  AutoFixture; consider `Ploch.TestingSupport.XUnit3.AutoMoq` (4.0.47 on nuget.org).
- `CrudEndpoints` uses `DbContext` but references only `Microsoft.EntityFrameworkCore.Abstractions`;
  it currently compiles transitively. It also references `Ardalis.Result.AspNetCore` without using
  it.
- The imported `obj/` assets were stale, so the review could not run a build. Expect to fix the
  build from scratch once the new solution exists.

### 3.4 Dependency and licensing decisions (ask the owner)

- **AutoMapper** is pinned at **14.0.0**, the last MIT version. Version 15 and later are
  commercially licensed. 14.0.0 had advisory GHSA-rvv3-g6hj-g44x
  ([PLO-324](https://linear.app/ploch/issue/PLO-324), done in ploch-common). `IMapper` is in every
  public handler constructor. For an open-source 1.0, consider a small mapping abstraction, with
  Mapperly or manual mapping as the default. **This is the owner's decision.**
- **FluentAssertions 8.x** is under the Xceed licence. It is only a test dependency, but confirm
  that it is acceptable for the open-source repository; the workspace already uses it.
- `Swashbuckle.AspNetCore.SwaggerGen` sits on top of `Microsoft.AspNetCore.OpenApi`, which is
  redundant on .NET 10. Decide which one to keep.

---

## 4. Rename map (proposed; confirm the open points with the owner)

`ploch-endpoints` is a **library family**, so follow `.claude/rules/project-naming.md` § Library
Families:

- The directory strips only `Ploch.`.
- The segment after `Endpoints` names a feature area.
- **Never** set `AssemblyName`, `RootNamespace` or `PackageId` by hand.
- Tests live under `tests/`.

| Old project | New project | New directory |
|---|---|---|
| `Ploch.Common.WebApi.Endpoints` | `Ploch.Endpoints` | `src/Endpoints/` |
| `Ploch.Common.WebApi.Endpoints.CrudEndpoints` | `Ploch.Endpoints.Crud` | `src/Endpoints.Crud/` |
| `...CrudEndpoints.MinimalApiEndpoints` | `Ploch.Endpoints.Crud.MinimalApi` | `src/Endpoints.Crud.MinimalApi/` |
| `...CrudEndpoints.FastEndpoints` | `Ploch.Endpoints.Crud.FastEndpoints` | `src/Endpoints.Crud.FastEndpoints/` |
| `Ploch.Common.WebApi` (OpenApi + QueryStringBinder) | **Open point.** Proposed: `Ploch.Endpoints.AspNetCore` | `src/Endpoints.AspNetCore/` |
| `Ploch.Common.Ardalis.Result` | **Fold** `ResultStatusExtensions` into `Ploch.Endpoints`, because both flavours need status mapping. Alternatively, use `Ardalis.Result.AspNetCore` in both flavours and delete the helper. | — |
| `...CrudEndpoints.IntegrationTests` | `Ploch.Endpoints.Crud.IntegrationTests` | `tests/Endpoints.Crud.IntegrationTests/` |
| `Ploch.Common.WebApi.Endpoints.Tests` (empty) | `Ploch.Endpoints.Tests`, with real tests, or delete it | `tests/Endpoints.Tests/` |
| `Ploch.Common.WebApi.Tests` | `Ploch.Endpoints.AspNetCore.Tests` (follows the open point) | `tests/Endpoints.AspNetCore.Tests/` |

Also:

- Rename the namespaces `Ploch.Common.WebApi.*` to `Ploch.Endpoints.*`, including the
  `InternalsVisibleTo` entries.
- Consider whether `Models` should become `Contracts`.
- Tidy the DI API. `ServiceCollectionRegistration` holds three public types in one file, and
  `MapEndpoints()` registers DI types rather than routes. Split the file and give the methods
  clearer names.
- Avoid `Ploch.Endpoints.Endpoints`-style stutter, and BCL-shadowing segments (see
  *Names That Collide* in `project-naming.md`).
- `Ploch.Endpoints` is **not yet taken on nuget.org** (checked 2026-10-13). Reserving the `Ploch.*`
  prefix is optional.

---

## 5. Repository layout and build files

Mirror ploch-common and ploch-data (`.claude/rules/project-structure.md`):

```
Ploch.Endpoints.slnx            # solution folders mirror the disk layout
src/  tests/  samples/  docs/  DocumentationSite/  change-log/  scripts/
Directory.Build.props  Directory.Packages.props  .editorconfig  .gitignore  .gitattributes
global.json  version.json  .config/dotnet-tools.json  NuGet.Config (if needed)
README.md  RELEASE_NOTES.md  CLAUDE.md  AGENTS.md  LICENSE
```

- **`Directory.Packages.props`.** Central Package Management that imports
  `../mrploch-development/dependencies/*.props`, as in ploch-common. CI clones
  `mrploch-development` next to the repository (`build-dotnet.yml`, step "Clone
  mrploch-development"). Add the `Ploch.Data.*` / `Ploch.Common` versions there if they are missing
  from `Ploch.Packages.props`. **No floating versions.**
- **`Directory.Build.props`.** Copy ploch-common's version and adapt it:
  - `RepositoryUrl` set to `https://github.com/mrploch/ploch-endpoints.git`.
  - `PackageLicenseExpression` set to Apache-2.0, once confirmed.
  - Keep: `Authors`, SourceLink, `.snupkg`, deterministic CI builds, XML docs for non-test projects,
    test-project auto-detection.
  - `TargetFrameworkVersion` set to `net10.0`.
  - Analysers: StyleCop, ErrorProne, Roslynator, Sonar.
- **`.editorconfig`.** Copy ploch-common's version, including `dotnet_diagnostic.IDE0058.severity =
  none` and its rationale (`code-quality.md`).
- **`version.json`.** Use NBGV, as ploch-common does (`publicReleaseRefSpec` for `main` and `v*`
  tags). Start at `1.0-prerelease`, or `0.1-prerelease` if the owner prefers.
- **`change-log/`.** One entry per change: `YYYY-MM-DD-plo-<n>-<slug>.md` (`change-log.md` rule).
  `release.yml` reads these entries.

---

## 6. GitHub workflows: match ploch-common and ploch-data

Copy each workflow from the reference repository and adapt it; **do not write them from scratch**.
ploch-data pins its actions to commit SHAs and ploch-common partly uses tags. Prefer the ploch-data
style: SHA-pinned, newest versions.

| Workflow | Copy from | What it does |
|---|---|---|
| `build-dotnet.yml` | `ploch-common/.github/workflows/build-dotnet.yml` (and compare with ploch-data's) | Runs on push to `main`, on PRs and on `workflow_dispatch`. Steps: `fetch-depth: 0` (NBGV), clone `mrploch-development`, restore/build/test with Coverlet coverage, SonarCloud scan (`SONAR_TOKEN`, `vars.SONAR_ORGANIZATION`, `vars.SONAR_PROJECT_KEY`), Codacy coverage upload (`CODACY_PROJECT_TOKEN`), test-results artefacts (`*.trx`), DocFX smoke build (`continue-on-error`), and prerelease `.nupkg`/`.snupkg` publishing to GitHub Packages (`GH_PACKAGES_TOKEN`) on `main`. Keep the test-scaffolding package filter from ploch-common `915afe0`. |
| `test_report.yml` | `ploch-data/.github/workflows/test_report.yml` | Runs on `workflow_run`. Publishes the test report check (`phoenix-actions/test-reporting`), named **Test Results**, which is a required check in ploch-common. |
| `release.yml` | `ploch-common/.github/workflows/release.yml` (see `ploch-common/CLAUDE.md` § Release Pipeline) | Runs on `workflow_dispatch` with `release_version`/`next_version`. Steps: `nbgv set-version`, Release build and test, annotated `v*` tag, publish to **nuget.org through Trusted Publishing** (`NuGet/login`, `id-token: write`, `vars.NUGET_USER`; no API key secret), release notes from `change-log/*.md`, GitHub Release, archive the change-log entries, and a bookkeeping **PR** with the next version (`GH_TOKEN` PAT, because branch protection blocks direct pushes). |
| `publish-docs.yml` | `ploch-common/.github/workflows/publish-docs.yml` | Builds DocFX and deploys it to GitHub Pages on push to `main`. Publishes PR previews to the `gh-pages-previews` branch, with `vars.DOCS_PREVIEW_BRANCH` and `vars.DOCS_PREVIEW_BASE_URL`. |
| `docs-preview-cleanup.yml` | ploch-common | Removes a PR's docs preview when the PR closes. |
| `qodana_code_quality.yml` / `code_quality.yml` | ploch-common / ploch-data | Qodana scan (`QODANA_TOKEN`). |
| CodeQL | ploch-data has the **`Analyze (csharp)`** check as a required check | Enable CodeQL (default setup, or an advanced workflow) for C#. |
| `.github/dependabot.yml` | ploch-data / ploch-common | Already copied; check its ecosystems and directories. |
| Optional | ploch-data `copilot-pr-pipeline.yml`, ploch-common `diagnose-gh-token.yml` | Ask the owner whether they are wanted. |

Other `.github` files to bring over from ploch-data:

- `CODEOWNERS`
- `ISSUE_TEMPLATE/`
- `FUNDING.yml`
- `copilot-instructions.md`
- `git-commit-instructions.md`
- The PR template, which is already present; check that it fits.

**External services the reference repositories use.** Each must be onboarded for `ploch-endpoints`,
and most need the owner:

- SonarCloud project and key
- Codacy repository and project token
- CodeFactor
- CodeRabbit
- Bito
- Qodana
- Coverage reporting through Codacy

---

## 7. GitHub repository settings (needs repository admin; partly the owner's job)

The current state of `mrploch/ploch-endpoints` (checked 2026-10-13):

- Public, default branch `main`.
- **No branch protection.**
- No licence.
- Description: "Web Api CRUD Endpoints".

Match ploch-common and ploch-data:

- **Branch protection on `main`** (classic protection, as in ploch-common):
  - Require a PR with **1 approving review**.
  - **Strict** status checks (branch must be up to date).
  - **Require conversation resolution.**
  - Admins not enforced, so the solo owner can merge.
  - Required checks, *once they have reported at least once*: `Test Results`,
    `SonarCloud Code Analysis`, `Codacy Static Code Analysis`, `CodeFactor`, `Analyze (csharp)`,
    and `CodeRabbit` if enabled.
  - Add the protection **after** the first green PR. Otherwise required checks that have never run
    block all merges.
- Merge settings: `delete_branch_on_merge: true`; squash commit title `COMMIT_OR_PR_TITLE`.
- About section:
  - Description, e.g. "Generic CRUD Web API endpoints (Minimal API and FastEndpoints) over
    Ploch.Data generic repositories".
  - Topics, e.g. `dotnet`, `aspnetcore`, `minimal-api`, `fastendpoints`, `crud`,
    `repository-pattern`, `entity-framework-core`.
  - Homepage: the docs URL.
- GitHub Pages:
  - Source: **GitHub Actions** workflow.
  - Custom domain following the existing pattern (`common.github.ploch.dev`,
    `data.github.ploch.dev`), e.g. **`endpoints.github.ploch.dev`**. The owner adds the DNS record.
- Secrets (names only; values come from the owner): `SONAR_TOKEN`, `CODACY_PROJECT_TOKEN`,
  `GH_TOKEN` (PAT that can trigger workflows), `QODANA_TOKEN`. `GH_PACKAGES_TOKEN` appears to be an
  organisation secret; confirm.
- Variables: `SONAR_ORGANIZATION`, `SONAR_PROJECT_KEY`, `NUGET_USER` (the nuget.org profile name),
  `DOCS_PREVIEW_BRANCH`, `DOCS_PREVIEW_BASE_URL`.
- **nuget.org Trusted Publishing policy** for `mrploch/ploch-endpoints`, workflow `release.yml`, and
  the package glob. **The owner sets this up.**
- Security: enable secret scanning, push protection and Dependabot alerts.

---

## 8. Packaging, README, DocFX and the sample app

- **NuGet packaging.** Each library project gets:
  - `Description` and `PackageTags`.
  - A **package README**: a per-package `README.md` packed with `PackageReadmeFile`. Check how
    ploch-common/ploch-data do it, and match.
  - The icon, if the reference repositories use one.

  Test projects and the sample stay `IsPackable=false`. Whether the `AspNetCore` helpers ship is
  [PLO-322](https://linear.app/ploch/issue/PLO-322); ask the owner.
- **Repository README.** Keep it to 150 lines or fewer (`rules.md`): what it is, a quick start for
  each flavour, the package table, links to the docs, and badges (build, SonarCloud, Codacy,
  nuget.org). `docs/libraries/common-webapi.md` is the starting draft; rename everything in it.
- **DocFX.** Use the `docfx-api-docs` skill, and copy `DocumentationSite/` (`docfx.json`, template,
  TOC, namespace overwrites) from ploch-data/ploch-common. Build with `--warningsAsErrors`, as
  ploch-common does ([PLO-446](https://linear.app/ploch/issue/PLO-446) is the precedent).
- **Sample app** (`.claude/rules/sample-apps.md`, `project-naming.md` § Sample Applications):
  - Put it in `samples/SampleApp/`, using names like `Ploch.Endpoints.SampleApp.Model`, `.Data`,
    `.Data.SQLite`, `.Api.WebApi` (one host for each flavour, or a switch).
  - It must reference the **published NuGet packages by default**. An opt-in
    `-p:UsePlochProjectReferences=true` switch is allowed.
  - It must run end-to-end: create, get, list (paged), update and delete over HTTP. Run it manually
    before the PR.
  - Model it on `ploch-data/samples/SampleApp`.

---

## 9. Suggested order of work

Split [PLO-776](https://linear.app/ploch/issue/PLO-776) into Linear sub-issues, one per bullet
group. Give each its own `<type>/plo-<n>-<slug>` branch and PR.

1. Decide the open points with the owner:
   - the `AspNetCore` helper project's name and whether it ships;
   - the AutoMapper strategy;
   - the licence;
   - the initial version.
2. Rename and restructure (§4, §5): the solution, build files and NuGet references, with no
   behaviour change. Get `dotnet build`/`dotnet test` green on a **clean clone with no sibling
   checkouts** apart from `mrploch-development`.
3. CI (§6), then onboard the external services, then the first green PR.
4. Fix the correctness defects (§3.1) with tests, one issue per defect where it makes sense.
5. Write the XML docs and clear the analyser warnings (§3.2,
   [PLO-343](https://linear.app/ploch/issue/PLO-343)).
6. Sample app, README, package READMEs and DocFX (§8).
7. Repository settings and branch protection (§7). Branch protection goes last, once the checks
   exist.
8. Run `release.yml` for the first prerelease or release, after the owner sets up Trusted
   Publishing.

For every PR:

- Run the full completion gate (`pr-checks-completion-gate.md`).
- Get the triple external AI review (`external-ai-review.md`).
- Keep the Notion and notes traceability (`work-traceability.md`).
