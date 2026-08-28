# Contributing to NYC-GAME

Thank you for your interest in contributing! This guide explains how to contribute safely and consistently.

## 🔧 Before You Commit

Please follow these steps **before every commit**:

1. **Save all files** — Make sure every open file is saved in your editor (Ctrl/Cmd + S). Unsaved changes will not be committed.
2. **Sync before commit** — Pull the latest changes from the remote and merge/rebase so your branch is up to date:
   ```bash
   git pull origin main
   ```
   Resolve any conflicts, then continue.
3. **Build & test** — Open the project in Unity and confirm it compiles without errors before committing.

## 💡 Ideas & Suggestions

Have a new idea (weapons, maps, mechanics, VFX)?

- Open a new **issue** with the `idea` label, or
- Write your idea into [`idea.md`](./idea.md) and open a pull request.

Please keep ideas clear and concrete: what it is, why it fits the game, and how it could be implemented.

## 📋 Basic Contribution Rules

- Work on a **feature branch** (e.g. `feature/new-gun`, `fix/reload-bug`), never directly on `main`.
- Keep commits **small and focused** with clear messages.
- Follow the existing code style and use **ScriptableObjects** for guns/modifiers.
- Run the project in **Unity 2022.3 LTS** to match the engine version.
- Be respectful and constructive in issues and reviews.
- Submit a pull request and wait for review before merging.

## 🚀 Submitting a Pull Request

1. Fork and clone the repo.
2. Create your feature branch.
3. Make your changes, then save all files and sync (`git pull`) before committing.
4. Push and open a PR describing your change.
