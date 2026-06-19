# Git Workflow — Locked Folder Prevention on Windows

## Problem

When Visual Studio has a solution open, it holds Windows file handles on `.cs`, `.csproj`,
and related files. During a `git checkout` or `git merge`, Git for Windows tries to delete
directories that no longer belong to the target branch. If any file handle inside a directory
is held by VS (or any other process), the directory deletion fails with `ERROR_SHARING_VIOLATION`.

Git's default behavior is to call `ask_yes_no_if_possible()`, which **blocks and waits for
interactive input** ("Should I try again? [y/N]"). When commands are run from an agent or
automated terminal context, this prompt is invisible to the user and the agent — the command
simply hangs until someone answers in the PS window. The consequence is that:

- The agent receives no output and cannot detect the block.
- Answering "no" repeatedly leaves behind empty (but physically present) ghost directories
  on disk that Git does not track.
- These dead directories are harmless to Git but clutter the solution tree and can confuse
  Visual Studio's file scanner.

## Permanent Fixes Applied

### 1. `GIT_TERMINAL_PROMPT=0` in PowerShell profile

Added to `C:\Users\RMold\OneDrive\One Drive Documents\WindowsPowerShell\Microsoft.PowerShell_profile.ps1`:

```powershell
# Suppress Git interactive prompts (prevents locked-folder deletion pauses)
$env:GIT_TERMINAL_PROMPT = 0
```

Effect: Git treats stdin as non-interactive in all future PowerShell sessions. The
`ask_yes_no_if_possible()` call auto-answers "no" immediately instead of blocking.
The locked-directory deletion still fails, but silently — no pause, no prompt.

> Also applied to the current session manually: `$env:GIT_TERMINAL_PROMPT = 0`

### 2. Per-repo Git config settings

Applied to the local repo config (`.git/config`):

```
git config core.fscache true
git config checkout.workers 1
```

| Setting | Effect |
|---|---|
| `core.fscache true` | Reduces repeated file-stat calls on Windows, lowering lock contention frequency |
| `checkout.workers 1` | Serializes file writes during checkout, preventing parallel-checkout races on locked files |

## Residual Ghost Directories

Even with `GIT_TERMINAL_PROMPT=0`, locked directories that Git cannot delete will remain on
disk as empty folders. They contain no files and are not tracked by Git, so they are safe to
remove at any time.

### Cleanup script (run after any checkout/merge where VS was open)

```powershell
# Remove empty untracked directories left by locked-folder failures
# Adjust $deadDirs to match any new ghost paths
$root = "C:\Users\RMold\OneDrive\One Drive Documents\SDC\SDC Git Repo\SDC.Schema"
$deadDirs = @(
	"SDC.Schema.Tests\Utility Classes",
	"SDC.Schema.Tests\UtilityClasses\AttributeInfo",
	"SDC.Schema.Tests\UtilityClasses\PropertyInfoOrdered",
	".github\upgrades\scenarios\newtonsoft-json-migration",
	"Docs"
)
foreach ($d in $deadDirs) {
	$full = "$root\$d"
	if (Test-Path $full) {
		$fileCount = (Get-ChildItem $full -Recurse -File).Count
		if ($fileCount -eq 0) {
			Remove-Item $full -Recurse -Force
			Write-Host "DELETED: $d"
		} else {
			Write-Host "SKIPPED (has $fileCount files — review before deleting): $d"
		}
	} else {
		Write-Host "ALREADY GONE: $d"
	}
}
```

> The script only deletes directories that contain **zero files**. Any directory with files
> is skipped and reported, preventing accidental data loss.

## Best Practice: Close the Solution Before Branch Switches

The most reliable prevention is:

1. In Visual Studio: **File → Close Solution**
2. In PowerShell: perform the `git checkout` / `git merge`
3. In Visual Studio: **File → Open → Project/Solution** to reopen

This releases all file handles before Git begins its tree manipulation, allowing normal
deletion of directories that belong to the old branch.

## Summary

| Layer | Mechanism | Status |
|---|---|---|
| PS profile `GIT_TERMINAL_PROMPT=0` | Silences interactive prompt globally | ✅ Applied |
| `core.fscache true` | Reduces stat-based lock hits | ✅ Applied (this repo) |
| `checkout.workers 1` | Prevents parallel-checkout races | ✅ Applied (this repo) |
| Close VS before branch switch | Eliminates the lock at the source | Best practice — manual |
| Post-merge cleanup script | Removes any residual empty ghost dirs | Available above |
