This part of the FPLedit is a simplified copy of the [DeepCloner](https://github.com/force-net/DeepCloner/) package. It is simplified (i.e. the .NET 4.x MSIL-based mechanism has been removed, also no shallow cloning support).

# DeepCloner

Library with extenstion to clone objects for .NET. It can deep or shallow copy objects. In deep cloning all object graph is maintained. Library actively uses code-generation in runtime as result object cloning is blazingly fast.
Also, there are some performance tricks to increase cloning speed (see tests below).
Objects are copied by its' internal structure, **no** methods or constructors are called for cloning objects. As result, you can copy **any** object, but we don't recommend to copy objects which are binded to native resources or pointers. It can cause unpredictable results (but object will be cloned).

You don't need to mark objects somehow, like Serializable-attribute, or restrict to specific interface. Absolutely any object can be cloned by this library. And this object doesn't have any ability to determine that he is clone (except with very specific methods).

Also, there is no requirement to specify object type for cloning. Object can be casted to inteface or as an abstract object, you can clone array of ints as abstract Array or IEnumerable, even null can be cloned without any errors.

## License

[MIT](https://github.com/force-net/DeepCloner/blob/develop/LICENSE) license