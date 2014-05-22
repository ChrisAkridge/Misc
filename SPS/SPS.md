# The Standard Programming Specification
This document specifies a standard for which all software development projects shall follow. This standard is designed for consistency, reliability, and simplicity of the software developed. This document defines universal project requirements, specification requirements and format, code formatting and naming conventions, coding paradigms, and the usage of test-driven development in projects. Ultimately, the goal of this document is to ensure that future programming projects are consistently well-defined, well-formatted, well-named, well-tested, and, most importantly of all, correct.

I'll do my level best to make this document as general, platform-agnostic, and language agnostic as I can, but ultimately this will reflect my desire for consistent C# software developed in Windows. You might do better to fork this document and modify it as you see fit.

## Table of Contents
1. Defining Specifications
	1. Why Write Specifications?
	2. Specifications are Living Documentation
	3. The Basic Format and Layout of Specifications
	4. Answering the Key Questions: What, How, and Why?
	5. Specifying the Logic
2. Project Code Layout
	1. What is a Code Layout?
	2. Writing a Code Layout
3. Development Conventions and Best Practices ([see also](http://msdn.microsoft.com/en-us/library/ms184412(v=vs.110).aspx))
	~ (this section will be completed once I read the above links)
4. Test-Driven Development
	1. The TDD Cycle
	2. What Measure is a Feature?
	3. Refactoring

## Section 1: Defining Specifications
### Part 1: Why Write Specifications

1.1.1 Many aspects of life are subjective and not always easy to define. Programming, despite its seeming complexity, is not one of those things. Because of its roots in math, programming and design is a concept that can be more clearly defined than many other aspects of life. Despite the unpredictability of users, their computers, the networks, the Internet, the hardware, or everything else, programs themselves can be well defined.

1.1.2 However, writing specifications is an often-forgotten aspect of programming. Joel Spolsky once likened it to flossing: everyone knows they should, but no one does ([source](http://www.joelonsoftware.com/articles/fog0000000036.html)). People might think writing specifications is too difficult or too time-consuming. Time is money, indeed, so why should we waste time on specifications? Let's just dive into our favorite editors and start hacking away! That is, until a few months later, where the code is a unfactored mess of different conventions, paradigms, and horrid intercoupled logic that can turn any elegant language feature into looking like a monkey wrote it.

1.1.3 Specifications are important. They tell you what the software does. Not in a vague, "well, this software reticulates these splines" or the buzzwordy "Our software combines NoSQL and jQuery with Mono to deliver an crowd-sourced, synergistic experience". It tells you exactly what the software does and how, with what data structures and methods. Specifications define the what, how, and why of software, and state it goals - the reason anybody would want to use it.

1.1.4 Writing specifications stills sounds like a lot of hard work, and it is. But it saves you time and trouble in the long run. Writing a specification - what the software does and, more importantly, **how** it does it - forces you to think through how the software will actually work before you even write a line of code. Fixing the design problems is also much easier here, and it's also a good time to give some thought into which libraries and tools you'd like to use.

### Section 2: Specifications Are Living Documentation
1.2.1 A specification can and should change as development progresses. A living specification is useful not only to the developers but to users and other developers. It can demonstrate how to use the software and provide examples. As the software becomes more and more complete, the specification slowly becomes documentation. The specification will not always be the best design at first and there will be problems that will arise with the design as the software is developed. The specification should be changed (with revisions or version control, of course) as the software's design changes.