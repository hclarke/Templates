Templates
=========

templete system for C#
it works if you don't screw up the syntax

##syntax

'@' surrounds variables and loops
'?' starts a separator
'$' closes loops and separators

##variables

you can access fields of the data object, or any loop variable

> hello, @name@!

together with data

> new Archangel() { name = "metatron" }

becomes

> hello, metatron!

##loops

you can loop through any IEnumerable

> @archangel:archangels@hello, @archangel.name@!
> $

with data

> new Archangels() {
>   archangels = new[] {
>     new Archangel() { name = "metatron" },
>     new Archangel() { name = "samael" },
>     new Archangel() { name = "azrael" }
>   }
> }

becomes

> hello, metatron!
> hello, samael!
> hello, azrael!
> 

##separators

you can insert text on every loop iteration except the last. good for commas and such.

> greetings: @archangel:archangels@@archangel.name@?, $$.

with data

> new Archangels() {
>   archangels = new[] {
>     new Archangel() { name = "metatron" },
>     new Archangel() { name = "samael" },
>     new Archangel() { name = "azrael" }
>   }
> }

becomes

> greetings: metatron, samael, azrael.
