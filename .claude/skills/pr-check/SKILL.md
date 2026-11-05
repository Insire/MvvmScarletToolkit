---
name: pr-check
description: Pre-PR check for dev -> master. Builds, tests, reviews the diff against project conventions, drafts the PR and gives you a pre-filled GitHub link.
argument-hint: [base branch, default master]
disable-model-invocation: true
---

# PR check (dev -> master)

Base branch: `$ARGUMENTS`. If empty, use `master`.

## 1. Collect the change set

```bash
git fetch origin
git status --short
git log --oneline origin/<base>..HEAD
git diff --stat origin/<base>...HEAD
```

If there are uncommitted changes, list them and ask whether to include them before continuing.

## 2. Build and test

```bash
dotnet build MvvmScarletToolkit.slnx -c Release
dotnet test --solution MvvmScarletToolkit.slnx -c Release
```

Report warnings introduced by the diff, not the existing ones. On a failure, stop and show the failing tests with their output.

## 3. Review the diff against the conventions

Read `git diff origin/<base>...HEAD` and check:

- [ ] **netstandard2.0 projects** (`Abstractions`, `MvvmScarletToolkit`, `Commands`, `Observables`): no APIs that are missing from netstandard2.0, such as `ArgumentNullException.ThrowIfNull`, `string.Contains(char)`, `KeyValuePair` deconstruction, `Index`/`Range`, `IAsyncEnumerable` without the polyfill, or `[NotNullWhen]` without a polyfill.
- [ ] **Messenger** handlers filter by sender or value.
- [ ] **UI work** in viewmodels goes through `Dispatcher.Invoke`, and commands are built through `IScarletCommandBuilder`.
- [ ] **New namespaces** in `MvvmScarletToolkit.Wpf` have an `XmlnsDefinition` in `Properties/AssemblyInfo.cs`.
- [ ] **New behaviors** have the `<remarks>` xmlns block.
- [ ] **WPF tests** use `StaRunner.Run` and derive from `TraceTestBase`.
- [ ] **Public behavior changes** come with an updated `src/<Package>/readme.md` and a new or extended sample.
- [ ] **Package versions** change only in `Directory.Packages.props`, with no `Version=` on `PackageReference` in a csproj.
- [ ] **Code style** follows `.editorconfig`: block-scoped namespaces and braces on new lines.

List each finding as `file:line - problem - suggested fix`. Don't fix anything without asking.

## 4. Draft the PR

- Title: a short imperative sentence, the same style as commit messages (e.g. `Add drag reorder behavior for ListBox`).
- Body: **Summary** (2-4 bullets), **Packages affected**, **Breaking changes** (or "none"), **Samples/docs updated**, **Tests**.

Show the title and body in a fenced markdown block so the user can copy them.

## 5. Hand off to GitHub

The GitHub CLI (`gh`) and the GitHub MCP aren't available, so don't create the PR yourself. Prepare everything the user needs to open it in the browser:

1. Check that `dev` is pushed: `git rev-list --count origin/dev..dev`. If the count isn't 0, say how many commits are unpushed and ask whether to run `git push origin dev`. Never push without confirmation.
2. Build a compare link that pre-fills the PR form. URL-encode the title and body, for example with `python -c "import urllib.parse,sys; print(urllib.parse.quote(sys.argv[1]))" "<text>"`:

   ```
   https://github.com/Insire/MvvmScarletToolkit/compare/<base>...dev?expand=1&title=<encoded title>&body=<encoded body>
   ```

   If the encoded URL is longer than about 7000 characters, leave out `&body=...` and tell the user to paste the body from the draft.
3. Give the user the link, and say that clicking **Create pull request** on that page finishes the step.
