---
title: Playing with reactive collections in C#
lang: en-us
date: 2018-01-02 21:59:09
tags:
---

Hello folks!

Few days ago I was writing some exercices in C# and I notice an interesting behavior about IEnumerable and IQueryable.
Observe the following code:

```csharp
using System;
using System.Linq;
using System.Collections;

namespace reactiveCollection
{
    class Program
    {
        static void Main(string[] args)
        {
            IEnumerable collection = Enumerable.Range(0,10);

            foreach (int item in collection)
            {
                Console.WriteLine(item);
            }

            Console.ReadLine();
        }
    }
}
```

Quite simple, and if we runt it in terminal, we get the following response:

```bash
root@1083f7cca7e3:/app# dotnet run
0
1
2
3
4
5
6
7
8
9
```

Well, I guess we all were expecting that response, but what if we change our collection structure from `IEnumerable` to `IQueryable`?

```csharp
using System;
using System.Linq;
using System.Collections;

namespace reactiveCollection
{
    class Program
    {
        static void Main(string[] args)
        {
            IEnumerable collection = Enumerable.Range(0,10).Select(x => x);

            foreach (int item in collection)
            {
                Console.WriteLine(item);
            }

            Console.ReadLine();
        }
    }
}
```

```bash
root@1083f7cca7e3:/app# dotnet run
0
1
2
3
4
5
6
7
8
9
```

Basically the same thing. Now let's put some interaction with the user inside the expression:

```csharp
using System;
using System.Linq;
using System.Collections;

namespace reactiveCollection
{
    class Program
    {
        static void Main(string[] args)
        {
            IEnumerable collection = Enumerable
                .Range(0,10)
                .Select(x => 
                {
                    Console.ReadLine();

                    return x;
                });

            foreach (int item in collection)
            {
                Console.WriteLine(item);
            }

            Console.ReadLine();
        }
    }
}
```

Result:

```bash
root@1083f7cca7e3:/app# dotnet run


0

1

2

3

4

5

6

7

8

9

```

Something interesting happens... I usually would expect pressing the Enter button 10 times for after that the `foreach` print the numbers each line like the previous code.

But is not what occurs... Each `foreach` interaction, the instruction passed to `Select` command (which is `Console.ReadLine()`) is executed... That happens because `Enumerables` (and by consequence `IQueryables`) only execute expressions when they are returned by `yield` (see more about `yield` bellow).

Another exemple about what`s happening. Observe the following code:

```csharp

using System;
using System.Linq;
using System.Collections;

namespace reactiveCollection
{
    class Program
    {
        static void Main(string[] args)
        {
            IEnumerable collection = Enumerable
                .Range(0,10)
                .Select(x => 
                {
                    Console.WriteLine($"---------------------------Executing inside the Select at { DateTime.Now.ToLongTimeString()}");

                    return x;
                });

            foreach (int item in collection)
            {
                Console.WriteLine($"Executing outsite the Select at { DateTime.Now.ToLongTimeString()}");
            }

            Console.ReadLine();
        }
    }
}

```

E o resultado:

```bash

root@1083f7cca7e3:/app# dotnet run
---------------------------Executing inside the Select at 15:00:12
Executing outsite the Select 15:00:13
---------------------------Executing inside the Select at 15:00:13
Executing outsite the Select 15:00:14
---------------------------Executing inside the Select at 15:00:14
Executing outsite the Select 15:00:15
---------------------------Executing inside the Select at 15:00:15
Executing outsite the Select 15:00:16
---------------------------Executing inside the Select at 15:00:16
Executing outsite the Select 15:00:17
---------------------------Executing inside the Select at 15:00:17
Executing outsite the Select 15:00:18
---------------------------Executing inside the Select at 15:00:18
Executing outsite the Select 15:00:19
---------------------------Executing inside the Select at 15:00:19
Executing outsite the Select 15:00:20
---------------------------Executing inside the Select at 15:00:20
Executing outsite the Select 15:00:21
---------------------------Executing inside the Select at 15:00:21
Executing outsite the Select 15:00:22

```

This results indicates that, each interaction, the internal method passed to `Select` statement is executed.

This behavior give us some advantages in terms of processing. If the passed method is, for example, a heavy processing call, it would be executed when needed. If the loop were for some reason interrupted, lot of processing would be avoided.

If there's need to execute the entire loop (and each preposition passed into `Select` statement) before iterate, just need to force it execution, like calling the `.ToList()` extension:


```csharp

using System;
using System.Linq;
using System.Collections;

namespace reactiveCollection
{
    class Program
    {
        static void Main(string[] args)
        {
            IEnumerable collection = Enumerable
                .Range(0,10)
                .Select(x => 
                {
                    Console.WriteLine($"---------------------------Executing inside the Select at { DateTime.Now.ToLongTimeString()}");

                    return x;
                })
                .ToList();

            foreach (int item in collection)
            {
                Console.WriteLine($"Executing outsite the Select at { DateTime.Now.ToLongTimeString()}");
            }

            Console.ReadLine();
        }
    }
}

```

And the result becomes:

```bash

root@1083f7cca7e3:/app# dotnet run
---------------------------Executing inside the Select at 15:01:21
---------------------------Executing inside the Select at 15:01:22
---------------------------Executing inside the Select at 15:01:23
---------------------------Executing inside the Select at 15:01:24
---------------------------Executing inside the Select at 15:01:25
---------------------------Executing inside the Select at 15:01:26
---------------------------Executing inside the Select at 15:01:27
---------------------------Executing inside the Select at 15:01:28
---------------------------Executing inside the Select at 15:01:29
---------------------------Executing inside the Select at 15:01:30
Executing outsite the Select 15:01:31
Executing outsite the Select 15:01:31
Executing outsite the Select 15:01:31
Executing outsite the Select 15:01:31
Executing outsite the Select 15:01:31
Executing outsite the Select 15:01:31
Executing outsite the Select 15:01:31
Executing outsite the Select 15:01:31
Executing outsite the Select 15:01:31
Executing outsite the Select 15:01:31

```

We can realize that the total time of both executions is basically the same. But the console output is completely different.

This is the source code of the `.Select` method inside [`System.Linq`](https://github.com/dotnet/corefx/blob/master/src/System.Linq/src/System/Linq/Select.cs). We can see the `yield return` instruction that basically indicates a point of return in each interaction, and interactions were made only when they are needed (accordingly with the size of collection or N times before a break statement).

```csharp

private static IEnumerable<TResult> SelectIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, int, TResult> selector)
{
    int index = -1;
    foreach (TSource element in source)
    {
        checked
        {
            index++;
        }

        yield return selector(element, index);
    }
}

```

### But why can we say this code is Reactive?

When we iterate a collection, like in our example, we can have two kinds of behaviors: Reactive and Pro-active.

Pro-active is when all the possible results are calculated before they were requested. It's what happened in our last loop example when we added `.ToList()` method call. All collection results were calculated before iterate them. That's why the following `foreach` loop shown only the `Console.Write` result in console.

But the Reactive aproach executes every item of collection and returns its results each interaction. We can say that there is a "Reaction" inside the collection that executes the current item at time.

### A more complex example

For this example we will use the [Pokémon](https://www.pokeapi.co) API because it's free and don't need any authentication. Let's implement a program that shows only the 10 first pokemons, but stop showing when the first "fire" type pokemon is listed. First, let's try the pro-active approach:

```csharp
using static Newtonsoft.Json.JsonConvert;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Diagnostics;

namespace reactiveCollection
{
    class Program
    {
        static void Main(string[] args)
        {
            var stopWatch = new Stopwatch();

            stopWatch.Start();

            using(var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                IEnumerable<Pokemon> pokemons = Enumerable
                .Range(1,11)
                .Select(x => 
                {
                    var result = client.GetAsync($"http://pokeapi.co/api/v2/pokemon/{x}").Result; 

                    return DeserializeObject<Pokemon>(result.Content.ReadAsStringAsync().Result);
                })
                .ToList();   

                foreach(var pokemon in pokemons)
                {
                    Console.WriteLine(pokemon.Name);

                    if(pokemon.Types.Any(x => x.Type.Name.ToLower() == "fire"))
                        break;
                }

            }

            stopWatch.Stop();

            Console.WriteLine($"Execution Duration: {stopWatch.Elapsed.Seconds} seconds");
        }
    }
    public class Pokemon
    {
        public string Name { get; set; }
        public IList<PokemonTypeSlot> Types { get; set; }

        public class PokemonTypeSlot
        {
            public int Slot { get; set; }
            public PokemonType Type { get; set; }
            public class PokemonType {
                public string Name { get; set; }
            }
        }
    }
}
```

Results:

```bash
root@1083f7cca7e3:/app# dotnet run
bulbasaur
ivysaur
venusaur
charmander
Execution Duration: 36 seconds
```

Now let's remove only the `.ToList()` and allow the API call only be made in each interaction (I will show only the part of code):

```csharp
...
                IEnumerable<Pokemon> pokemons = Enumerable
                .Range(1,11)
                .Select(x => 
                {
                    var result = client.GetAsync($"http://pokeapi.co/api/v2/pokemon/{x}").Result; 

                    return DeserializeObject<Pokemon>(result.Content.ReadAsStringAsync().Result);
                });   

                foreach(var pokemon in pokemons)
                {
                    Console.WriteLine(pokemon.Name);

                    if(pokemon.Types.Any(x => x.Type.Name.ToLower() == "fire"))
                        break;
                }
...
```

The result is very different:

```bash
root@1083f7cca7e3:/app# dotnet run
bulbasaur
ivysaur
venusaur
charmander
Execution Duration: 10 seconds
```

Of course there are better ways to retrieve the same info from the API, but my intend is to show the concept.

### Conclusion

Reactive or Pro-active aproachs are diffents ways to obtain results in a timeline. It's not right to say one is better than other, because it depends by what is needed to be done. The most important thing here is to undestand their differences.

If you wish, you can check my code here [aqui](https://github.com/LuizAdolphs/blog/tree/master/codes/reactiveCollection)