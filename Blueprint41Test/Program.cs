﻿using Blueprint41;
using Blueprint41.Core;
using Blueprint41.Gremlin;
using Blueprint41.Modeller.Schemas;
using org.opencypher.gremlin.translation;
using org.opencypher.gremlin.translation.groovy;
using org.opencypher.gremlin.translation.translator;
using System;
using System.IO;
using System.Windows.Forms;
using Datastore.Manipulation;

namespace Blueprint41Test
{
    public class MovieModel : DatastoreModel<MovieModel>
    {

        public override GraphFeatures TargetFeatures => GraphFeatures.Neo4j;

        protected override void SubscribeEventHandlers()
        {

        }

        [Version(0, 0, 0)]
        public void Initialize()
        {
            FunctionalIds.Default = FunctionalIds.New("Org", "O_", IdFormat.Numeric);

            Entities.New("Film")
                .AddProperty("Uid", typeof(string), false, IndexType.Unique)
                .SetKey("Uid", true)
                .AddProperty("title", typeof(string))
                .AddProperty("tagline", typeof(string))
                .AddProperty("release", typeof(int));

            Entities.New("Actor")
                 .AddProperty("Uid", typeof(string), false, IndexType.Unique)
                .SetKey("Uid", true)
                .AddProperty("name", typeof(string))
                .AddProperty("born", typeof(int));

            Relations.New(Entities["Actor"], Entities["Film"], "PERSON_ACTED_IN_FILM", "ACTED_IN")
                .SetInProperty("ActedFilms", PropertyType.Collection)
                .SetOutProperty("Actors", PropertyType.Collection);

            Relations.New(Entities["Actor"], Entities["Film"], "PERSON_DIRECTED_FILM", "DIRECTED")
                .SetInProperty("DirectedFilms", PropertyType.Collection)
                .SetOutProperty("Directors", PropertyType.Collection);

            Relations.New(Entities["Actor"], Entities["Film"], "PERSON_PRODUCED_FILM", "PRODUCED")
                .SetInProperty("ProducedFilms", PropertyType.Collection)
                .SetOutProperty("Producers", PropertyType.Collection);

            Relations.New(Entities["Actor"], Entities["Film"], "PERSON_WROTE_FILM", "WROTE")
                .SetInProperty("FilmsWrited", PropertyType.Collection)
                .SetOutProperty("Writers", PropertyType.Collection);
        }


        [Version(0, 0, 1)]
        public void RefactorModel()
        {
            Entities["Actor"].Properties["name"].Refactor.Rename("fullname");
            Entities["Actor"].Properties["born"].Refactor.Deprecate();

            Relations["PERSON_DIRECTED_FILM"].Refactor.Rename("ACTOR_DIRECTED_FILM");

        }
    }

    class Program
    {
        //[STAThread]
        static void Main(string[] args)
        {
            string uri = "bolt://localhost:7687";
            string username = "neo4j";
            string pass = "neo";
            //"bolt://localhost:7687", "neo4j", "neo"


            //PersistenceProvider.Initialize(typeof(MovieModel), hostname, port, authKey, database, collection);
            PersistenceProvider.Initialize(typeof(MovieModel), uri, username, pass);


            MovieModel model = new MovieModel();
            model.Execute(true);


            using (Transaction.Begin(true))
            {
                Film matrix = new Film
                {
                    title = "The Matrix",
                    release = 1999,
                    tagline = "Welcome to the Real World"
                };

                Actor keanu = new Actor()
                {
                    fullname = "Keanu Reeves"
                };

                keanu.ActedFilms.Add(matrix);

                Transaction.Commit();
            }

            //string cypherText = File.ReadAllText(Environment.CurrentDirectory + "/moviegraph.txt");

            //string[] createcyphers = cypherText.Split(new string[] { "CREATE" }, StringSplitOptions.RemoveEmptyEntries);
            //string c = "";
            //foreach (string cy in createcyphers)
            //{
            //    c += "CREATE " + cy.Trim() + " ";
            //}

            Console.ReadLine();

            return;

            string cypher = null;
            do
            {
                Console.WriteLine("Enter new cyper string:");
                cypher = Console.ReadLine();

                if (cypher == "q")
                    break;

                if (string.IsNullOrEmpty(cypher.Trim()))
                {
                    Console.WriteLine("Enter new cyper string:");
                    continue;
                }

                Console.WriteLine("========Cosmos=========");
                Console.WriteLine(Translate.ToCosmos(cypher));
                Console.WriteLine("=================");

                Console.WriteLine();
                Console.WriteLine();

                //CypherAst ast;
                //try
                //{
                //    ast = CypherAst.parse(cypher);
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine(ex.Message);
                //    continue;
                //}


                //Translator groovyTranlator = Translator.builder().gremlinGroovy().build();
                //Translator cosmosTranlator = Translator.builder().gremlinGroovy().build(TranslatorFlavor.cosmosDb());
                //Translator neptuneTranlator = Translator.builder().gremlinGroovy().build(TranslatorFlavor.neptune());

                //string g, c, n;

                //Console.WriteLine("========Groovy=========");
                //Console.WriteLine((g = ast.buildTranslation(groovyTranlator).ToString()));
                //Console.WriteLine("=================");

                //Console.WriteLine("========Neptune=========");
                //Console.WriteLine((n = ast.buildTranslation(neptuneTranlator).ToString()));
                //Console.WriteLine("=================");

                //Console.WriteLine("========Cosmos=========");
                //Console.WriteLine((c = ast.buildTranslation(cosmosTranlator).ToString()));
                //Console.WriteLine("=================");

                //Console.WriteLine();
                //Console.WriteLine();

                //Console.WriteLine("Type g (groovy), n (neptune) or c (cosmos) to copy to clipboard. Or antyhing to cancel");

                //string toCopy = Console.ReadLine();

                //if (toCopy == "g")
                //{
                //    Clipboard.SetText(g);
                //    Console.WriteLine("Copied to clipboard");
                //}

                //else if (toCopy == "c")
                //{
                //    Clipboard.SetText(c);
                //    Console.WriteLine("Copied to clipboard");
                //}

                //else if (toCopy == "n")
                //{
                //    Clipboard.SetText(n);
                //    Console.WriteLine("Copied to clipboard");
                //}

            } while (cypher != "q");


            //TranslationFacade cfog = new TranslationFacade();
            //string gremlin = cfog.toGremlinGroovy(cypherText);
            //Console.WriteLine(gremlin);

            //TranslationFacade cfog = new TranslationFacade();
            //string gremlin = cfog.toGremlinGroovy("MATCH (cloudAtlas {title: \"Cloud Atlas\"})<-[:DIRECTED]-(directors) RETURN directors.name");
            //Console.WriteLine(gremlin);

            // Console.ReadLine();
        }
    }
}
