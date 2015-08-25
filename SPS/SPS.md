# The Standard Programming Specification

## Project Phases

### Phase 1: Specification
The first phase involves writing a specification describing, in general, the problem to be solved, and how the software will solve it. The specification need not be complex or rigourous; it merely needs to define the software and how it should work, as well as what technologies (language, framework, IDE) will be used by the software. Avoid specifying the structure of the actual code too deeply; that will be specified more in-depth in the next phase. Complex data structures can be defined here if needed.

### Phase 2: Code Map
The second phase involves producing a high-level overview of what the code will be composed of (namespaces, classes, methods, etc). This overview generally maps required functionality listed in the specification to a certain class or method. Each element in a code map should describe information about itself. Methods that perform particularly tricky operations might be described in the code map with psuedocode implementations.

For C# projects, a code map may take the following form:

* Assembly (name, description)
  * Namespace (name, description)
    * Class (name, access level, static/instance, abstract/sealed, inherits from, implements interface, description)
	   * Constant (name, type, value, access level)
	   * Field (name, type, static/instance, readonly/mutable, description)
	   * Property (name, type, access level, static/instance, virtual/abstract/sealed, has getter/setter, getter/setter access level, description)
	   * Autoproperty (name, type, access level, static/instance, virtual/abstract/sealed, has setter, getter/setter access level, description)
	   * Event (name, event handler delegate type, access level, description)
	   * Constructor (access level, arguments, static/instance, calls base constructor?, description)
	   * Method (name, access level, arguments, return type, static/instance, abstract/virtual/sealed, description)
	   * Generic Method (name, access level, generic type parameters, generic type constraints, arguments, static/instance, abstract/virtual/sealed, description)
	* Generic Class (name, access level, generic type parameters, generic type constraints, static/instance, abstract/sealed, inherits from, implements interface, description)
	* Delegate Type (name, access level, return type, arguments, description)
	* Struct (name, access level, implements interface, description)
	* Generic Struct (name, access level, generic type parameters, generic type constraints, implements interface, description).
	* Enumeration (name, access level, type of member, description)
		* Enumeration Member (name, value, description)
	* Interface (name, access level, implements interface, description)
	* Generic Interface (name, access level, generic type parameters, generic type constraints, implements interface, description)
	
An example partial class map from the SMLimitless project is show below:
* assembly "SMLimitless.exe"
  * namespace SMLimitless.Graphics
    * public class StaticGraphicsObject : IGraphicsObject: A graphics object made of a single texture
	  * private bool isLoaded: A flag set when the object's data is loaded.
	  * private bool isContentLoaded: A flag set when the object's texture is loaded.
	  * private string filePath: Path to the texture's image.
	  * private Texture2D texture: The texture of the object.
	  * public StaticGraphicsObject()
	  * internal Rectangle CgoSourceRect {get; set;}
	  * internal ComplexGraphicsObject CgoOwner {get; set;}
	  * public void Load(string filePath)
	  * public void Load(string filePath, DataReader config)
	  * public void LoadContent()
	  * public void Update()
...

The class map may be as concise or as general as necessary.