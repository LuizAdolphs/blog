---
title: 'Brincando com coleções reativas em c#'
date: 2017-12-12 18:16:50
tags:
---

Olá pessoal!

Esses dias estava desenvolvendo uns exercícios em C#, até que em um deles notei um comportamento interessante de IEnumerable e IQueryable. Observe o seguinte código:

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

E se executarmos no terminal, a seguinte resposta é gerada:

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

Bom, a resposta gerada era o que esperávamos... Mas o que acontece se trocarmos o `IEnumerable` por `IQueryable`?

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

Basicamente a mesma coisa. Vamos agora colocar uma 
interação do usuário na expressão:

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

Resultado:

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

Algo interessante acontece... Eu normalmente esperaria pressionar 10x o Enter para então depois o `foreach` printar os números um por linha como nos testes anteriores. 

Mas não é o que ocorre… A cada interação do `foreach`, a instrução de dentro do `Select` (isso é, o `Console.Readline()`) é executada… Isso acontece porque `Enumerables` (por consequência, `IQueriables`) só executam as expressões quando solicitadas através do `yield` (veja mais sobre o `yield` abaixo). 

Um outro exemplo do que está acontecendo. Observa o seguinte código:

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
                    Console.WriteLine($"---------------------------Executando de dentro do Select da data { DateTime.Now.ToLongTimeString()}");

                    return x;
                });

            foreach (int item in collection)
            {
                Console.WriteLine($"Executando de fora do Select { DateTime.Now.ToLongTimeString()}");
            }

            Console.ReadLine();
        }
    }
}

```

E o resultado:

```bash

root@1083f7cca7e3:/app# dotnet run
---------------------------Executando de dentro do Select da data 15:00:12
Executando de fora do Select 15:00:13
---------------------------Executando de dentro do Select da data 15:00:13
Executando de fora do Select 15:00:14
---------------------------Executando de dentro do Select da data 15:00:14
Executando de fora do Select 15:00:15
---------------------------Executando de dentro do Select da data 15:00:15
Executando de fora do Select 15:00:16
---------------------------Executando de dentro do Select da data 15:00:16
Executando de fora do Select 15:00:17
---------------------------Executando de dentro do Select da data 15:00:17
Executando de fora do Select 15:00:18
---------------------------Executando de dentro do Select da data 15:00:18
Executando de fora do Select 15:00:19
---------------------------Executando de dentro do Select da data 15:00:19
Executando de fora do Select 15:00:20
---------------------------Executando de dentro do Select da data 15:00:20
Executando de fora do Select 15:00:21
---------------------------Executando de dentro do Select da data 15:00:21
Executando de fora do Select 15:00:22

```

Este resultado nos indica que, a cada interação, o método interno do `Select` é executado.

Este comportamento nos dá diversas vantagens em termos de processamento. Se o método interno, por exemplo, fosse uma chamada um pouco mais pesada em termos de recursos computacionais, ela seria executada sob necessidade. Se por ventura o loop fosse parado no meio, processamento desnecessário seria evitado.

Caso fosse preciso executar todo o método `Select` antes de percorrer-lo, basta apenas forçar a interação dele com métodos de transformação, como por exemplo o `.ToList()`:


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
                    Console.WriteLine($"---------------------------Executando de dentro do Select da data { DateTime.Now.ToLongTimeString()}");

                    return x;
                })
                .ToList();

            foreach (int item in collection)
            {
                Console.WriteLine($"Executando de fora do Select { DateTime.Now.ToLongTimeString()}");
            }

            Console.ReadLine();
        }
    }
}

```

E o resultado vira:

```bash

root@1083f7cca7e3:/app# dotnet run
---------------------------Executando de dentro do Select da data 15:01:21
---------------------------Executando de dentro do Select da data 15:01:22
---------------------------Executando de dentro do Select da data 15:01:23
---------------------------Executando de dentro do Select da data 15:01:24
---------------------------Executando de dentro do Select da data 15:01:25
---------------------------Executando de dentro do Select da data 15:01:26
---------------------------Executando de dentro do Select da data 15:01:27
---------------------------Executando de dentro do Select da data 15:01:28
---------------------------Executando de dentro do Select da data 15:01:29
---------------------------Executando de dentro do Select da data 15:01:30
Executando de fora do Select 15:01:31
Executando de fora do Select 15:01:31
Executando de fora do Select 15:01:31
Executando de fora do Select 15:01:31
Executando de fora do Select 15:01:31
Executando de fora do Select 15:01:31
Executando de fora do Select 15:01:31
Executando de fora do Select 15:01:31
Executando de fora do Select 15:01:31
Executando de fora do Select 15:01:31

```

Podemos perceber que o tempo total entre as duas execuções é basicamente o mesmo. Mas o output para o console é bem diferente.

Esta é o codigo fonte do método `.Select` dentro de [`System.Linq`](https://github.com/dotnet/corefx/blob/master/src/System.Linq/src/System/Linq/Select.cs). É possível observar a utilização do `yield return` que indica, basicamente, que aquele ponto é o ponto de retorno para o interação corrente, isso é, o retorno é executado quantas vezes for necessário de acordo com a quantidade da coleção.

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

### Mas por que podemos dizer que este código é Reativo?

Quando tratamos de consultar uma coleção, que é nosso exemplo, podemos ter dois comportamentos possíveis: Reativo e Pró-ativo.

O pró-ativo é o comportamento que calcula os resultados possíveis antes mesmo de serem requisitados. É o que aconteceu no nosso ultimo exemplo quando adicionamos o `.ToList()`. Toda a coleção foi iterada e calculada de modo que no `foreach` subsequente apenas o `Consolte.Write` de dentro do foreach foi executado.

Já a abordagem Reativa vai executando cada item da coleção e obtendo seu resultado à cada interação. Podemos dizer ao iterar há uma "Reação" interna de dentro da coleção que executa a posição corrente (e obtém seu resultado na hora, não anteriormente).

### Um exemplo mais complexo

Para este exemplo estou usando a API do [Pokémon](https://www.pokeapi.co) por ser free e não necessitar autenticação. Vamos implementar a necessidade de listar os 10 primeiros pokémons, sendo que a listagem deve cessar assim que encontrar o primeiro pokémon do tipo "fogo". Primeiro, vamos usar a abordagem pró-ativa:

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

            Console.WriteLine($"Tempo de execução: {stopWatch.Elapsed.Seconds} segundos");
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

Resultado:

```bash
root@1083f7cca7e3:/app# dotnet run
bulbasaur
ivysaur
venusaur
charmander
Tempo de execução: 36 segundos
```

Agora, vamos apenas tirar o `.ToList()` e permitir a chamada à API por interação (mostrarei só o trecho):

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

O resultando é bem diferente:

```bash
root@1083f7cca7e3:/app# dotnet run
bulbasaur
ivysaur
venusaur
charmander
Tempo de execução: 10 segundos
```

Vale lembrar que certamente há melhores maneiras de consumir esta API e obter o mesmo resultado, mas desenvolvi assim para explicar melhor o conceito.

### Conclusão

Abordagens Reativas ou Pró-ativas são meios diferentes de se obter resultados numa linha de tempo. É errado perguntar quais das duas é melhor, pois dependendo do contexto é interessante utilizar um ou outro. O importante aqui é saber as diferenças e possibilidades de se trabalhar com ambas.

Caso você queira, deixei meu código [aqui](https://github.com/LuizAdolphs/blog/tree/master/codes/reactiveCollection)