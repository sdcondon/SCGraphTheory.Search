# In this folder..

An alternative version of the interfaces that adds the exact edge collection (of a node) type as a generic type parameter.
This means that algorithms can use the type directly rather than via its interface - avoiding boxing if it happens to be a value type.

Included because I was interested in the overall impact on performance.
As it turns out, (IMO) it doesn't make enough of a difference to be worth the extra complexity right now (turns out the runtime is pretty good at dealing with short-lived objects - go figure..).
May still be something I explore further, should I find the time and inclination.
