namespace LuckyWords.Migrations
{
    using Entities;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.IO;
    using System.Linq;
    using System.Text;

    internal sealed class Configuration : DbMigrationsConfiguration<LuckyWords.Context>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "LuckyWords.Context";
        }

        protected override void Seed(LuckyWords.Context context)
        {
            foreach (string initialWord in File.ReadAllLines("InitialWordsList.txt", Encoding.UTF8))
                context.InitialWordsList.AddOrUpdate(word => word.Word, new InitialWord() { Word = initialWord });

            foreach (string dictionaryWord in File.ReadAllLines("Dictionary.txt", Encoding.UTF8))
                context.Dictionary.AddOrUpdate(word => word.Word, new DictionaryWord() { Word = dictionaryWord });
        }
    }
}
