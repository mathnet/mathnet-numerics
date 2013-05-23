Contribute to Math.NET Numerics
===============================

Math.NET Numerics is driven by the community and contributors like you. I'm excited that you're interested to help us move forward and improve Numerics. We usually accept contributions and try to attribute them properly. 
Nevertheless, it is essential that we keep the library consistent, focused and mathematically accurate but still easy to use. Be prepared that a maintainer may ask you to apply some changes to your pull request before accepting it. Have a look at the following tips to get started quickly. I'm looking forward to your pull requests! Thanks!

&mdash; *Christoph Rüegg (@cdrnet), Maintainer*

## Getting Started

- Make sure you have a [GitHub account](https://github.com/signup/free), it's free.
- Fork the [mainline repository](https://github.com/mathnet/mathnet-numerics) on GitHub ([how to](https://help.github.com/articles/fork-a-repo)).

We use the [Fork & Pull Model](https://help.github.com/articles/using-pull-requests/), as common for GitHub projects. If you've already contributed to another GitHub project then you're all set. If not, [here is another  introduction](https://gun.io/blog/how-to-github-fork-branch-and-pull-request/). If you're new to git, don't worry. Git is very powerful but can be indeed overwhelming at first. We're here to help if something comes up.

We may ask you to squash your commits together in some rare cases. If so, [here is how](http://gitready.com/advanced/2009/02/10/squashing-commits-with-rebase.html). 

**New Files:**
When adding or renaming files, please make sure you also add them *as links* to the Portable project. This is a bit tedious but we have not found a better solution yet.

**Separate Branch per Pull Request:**
We recommend that you create a separate branch for each pull request, as opposed to using master. This isolates them properly and makes it much easier to continue working on a pull request even after it has been opened on GitHub. Remember that GitHub automatically includes all future commits of the same branch to the pull request.

**Focused:**
We prefer a couple small pull requests over a single large one that targets multiple things at once.

### When fixing a bug ...

If you have a good idea how to fix it, directly open a pull request. Otherwise you may want to open an issue first (at GitHub) and discuss it there. If you can reproduce the bug with simple enough code, please consider adding a Unit Test that fails before but succeeds after the fix.

### When extending features ...

If you're extending some feature which is similar and close to existing code, for example add a new correlation function in addition to the existing `Pearson` correlation coefficient, it's fine to directly open a pull request. We're likely to accept such pulls.

### When adding new features ...

If you intend to add completely new features, e.g. some spatial routines for geometrical transformations, we recommend to [talk to us](http://mathnetnumerics.codeplex.com/discussions) first. This is mostly to avoid wasting your time in case we decide not to accept it, or require major refactoring. If the features is quite small it is perfectly fine to just open a pull request though.

Note that your work does not need to be finished or complete at the time you open the pull request (but please mention this in the message), as GitHub pull requests are an excellent tool to discuss an early prototype or skeleton before it is fully implemented.

## What to Avoid

**Code Reformatting and Refactoring:**
Please avoid starting with a major refactoring or any code reformatting without talking to us first.

**Breaking Compatibility:**
We try to follow [semantic versioning](http://semver.org/), meaning that we cannot break compatibility until the next major version. Since Numerics intentionally permits straight access to raw algorithms, a lot of member declarations are public and thus cannot be modified.

**Merges:**
If your pull requests contains a merge commit it is a strong indication that something went wrong. Please make sure your commits sit directly on top of a recent mainline master.

**Diverging Branches:**
I've seen a couple forks where someone just goes on changing this and that and over time completely diverges from mainline. That's perfectly fine to do, but bringing anything from there back to mainline will become very challenging. Note that repeated merging from mainline does *not* prevent diverging at all but may actually make resolving them worse. If you consider to contribute any of your work back we recommend not to let your branch diverge too much from mainline.