Maintaining Math.NET Numerics
=============================

*Note: This document is only relevant for the maintainers of this project*

When creating a new release
---------------------------

Repository:

- Update RELEASENOTES file with relevant changes, attributed by contributor (if external). Set date.
- Update CONTRIBUTORS file (via `git shortlog -sn`)
- Create and publish a new annotated git tag for the release, e.g. `git tag -a v2.5.0 -m "v2.5.0"`
- Consider to resync the subtree in the native provider repository.
- Consider to update the repository mirrors at codeplex, gitorious and google ([how to](http://christoph.ruegg.name/blog/2013/1/26/git-howto-mirror-a-github-repository-without-pull-refs.html)).

Publish:

- Generate manual release build from internal TeamCity
- Update release notes in NuGet packages (using package explorer)
- Create new codeplex release, attach Zip files
- Upload NuGet packages to the NuGet Gallery

Misc:

- Consider a tweet via [@MathDotNet](https://twitter.com/MathDotNet)
- Consider a post to the [Google+ site](https://plus.google.com/112484567926928665204)
- Update Wikipedia release version+date for the [Math.NET Numerics](http://en.wikipedia.org/wiki/Math.NET_Numerics) and [Comparison of numerical analysis software](http://en.wikipedia.org/wiki/Comparison_of_numerical_analysis_software) articles.
- Regenerate api reference (at numerics.mathdotnet.com/api) using [docu](https://github.com/cdrnet/docu) and deploy to [website](http://numerics.mathdotnet.com/api/).
- Update documentation if necessary
- Consider blog post about changes
