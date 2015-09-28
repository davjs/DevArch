# DevArch - pre-alpha
## Architechture for anti-architechts

DevArch provides an alternative method for creating architecture diagrams.

Following in the footsteps of reverse-architechting, DevArch aims to provide one button press diagram generation.

However, instead of allowing the developer to later modify the generated diagram to their liking, DevArch relies on smart filtering algorithms that allows the user to customize *how* the diagram is generated.

These customizations, or rather, *diagram definitions*, can then be checked in together with the sourcecode. 

This means that anyone with the sourcecode are able to regenerate diagrams that are tailored for comunicating the original intent.

####DevArch makes architechting more formal, more explicit, faster and easier.

### Current architechture, as generated by DevArch:
![Alt text](/Current arch.PNG?raw=true "Optional Title")

Generated using the settings:
 - Pluralize name patterns
 - Pluralize base class patterns
 - Depth: 3
 - Treat chained dependencies as linnear ones
 - Skip single containers

###To try it out
Set the project "Standalone" to current startup project and press F5 to view a diagram of the current architechture.

###Todo:
* **Filters to add:**
* Reference count filter
* Lines of code count filter
* **Patterns to find:**
* Facade
* Singleton
* Vertical layers
* **Enable generation of class diagrams**
