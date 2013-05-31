Contribute to Math.NET Numerics
===============================

Math.NET Numerics is driven by the community and contributors like you. I'm excited that you're interested to help us move forward and improve Numerics. We usually accept contributions and try to attribute them properly, provided they keep the library consistent, focused and mathematically accurate. Have a look at the following tips to get started quickly. I'm looking forward to your pull requests! Thanks!

&mdash; *Christoph Rüegg (@cdrnet), Maintainer*

## Getting Started

- Make sure you have a [GitHub account](https://github.com/signup/free), it's free.
- Please configure a proper name and email address in git ([how to](https://help.github.com/articles/set-up-git)).
- Fork the [mainline repository](https://github.com/mathnet/mathnet-numerics) on GitHub ([how to](https://help.github.com/articles/fork-a-repo)).

We use the [Fork & Pull Model](https://help.github.com/articles/using-pull-requests/), as common for GitHub projects. If you've already contributed to another GitHub project then you're all set. If not, [here is another  introduction](https://gun.io/blog/how-to-github-fork-branch-and-pull-request/).

**New Files:**
When adding or renaming files, please make sure you also add them *as links* to the Portable project. This is a bit tedious but we have not found a better solution yet.

**Separate Branch per Pull Request:**
We recommend that you create a separate branch for each pull request, as opposed to using master. This makes it much easier to continue working on a pull request even after it has been opened on GitHub. Remember that GitHub automatically includes all future commits of the same branch to the pull request.

**Focused:**
We prefer a couple small pull requests over a single large one that targets multiple things at once.

### When fixing a bug ...

If you have a good idea how to fix it, directly open a pull request. Otherwise you may want to open an issue first (at GitHub) and discuss it there. If you can reproduce the bug with simple enough code, please consider adding a Unit Test that fails to confirm the bug.

### When extending features ...

If you're extending some feature which is similar and close to existing code, for example adding a new correlation function in addition to the existing Pearson correlation coefficient, it's fine to directly open a pull request. We're likely to accept such pulls.

### When adding new features ...

If you intend to add completely new features, say some spatial routines for geometrical transformations, we recommend to [talk to us](http://mathnetnumerics.codeplex.com/discussions) first. This is mostly to avoid wasting your time in case we decide not to accept it, or require major refactoring. If the features is quite small it is perfectly fine to just open a pull request though. Sometimes it's easier to just show code instead of lengthy explanations.

Note that your work does not need to be finished or complete at the time you open the pull request (but please do mention this in the message), as GitHub pull requests are an excellent tool to discuss an early prototype or skeleton before it is fully implemented.

## What to Avoid

**Code Reformatting and Refactoring:**
Please avoid starting with a major refactoring or any code reformatting without talking to us first.

**Breaking Compatibility:**
We try to follow [semantic versioning](http://semver.org/), meaning that we cannot break compatibility until the next major version. Since Numerics intentionally permits straight access to raw algorithms, a lot of member declarations are public and thus cannot be modified. Instead of breaking compatibility, it is often possible to create a new better version side by side and mark the original implementation as obsolete and scheduled for removal on the next major version.

**Merges:**
Please avoid merging mainline back into your pull request branch. If you need to leverage some changes recently added to mainline, consider to rebase instead. In other words, please make sure your commits sit directly on top of a recent mainline master.
