using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.Interfaces.SampleData;

namespace SysKit.ODG.SampleData
{
    public class SampleDataService : ISampleDataService
    {
        public ReadOnlyCollection<string> FirstNames { get; }
        public ReadOnlyCollection<string> LastNames { get; }
        public ReadOnlyCollection<string> GroupNames { get; }
        public ReadOnlyCollection<string> DepartmentNames { get; }
        public ReadOnlyCollection<string> CountryNames { get; }
        public ReadOnlyCollection<string> CityNames { get; }
        public ReadOnlyCollection<string> CompanyNames { get; }

        private readonly Random _randomGen = new Random();

        public SampleDataService()
        {
            FirstNames = createSampleCollection("firstName.csv");
            LastNames = createSampleCollection("lastName.csv");
            GroupNames = createSampleCollection("groupName.csv");
            DepartmentNames = createSampleCollection("departmentName.csv");
            CountryNames = createSampleCollection("countryName.csv");
            CityNames = createSampleCollection("cityName.csv");
            CompanyNames = createSampleCollection("companyName.csv");
        }

        public string GetRandomValue(IList<string> sampleCollection)
        {
            return sampleCollection[_randomGen.Next(sampleCollection.Count)];
        }

        public string GetRandomValue(IList<string> primaryCollection, IList<string> secondaryCollection)
        {
            var firstValue = GetRandomValue(primaryCollection);
            var secondValue = GetRandomValue(secondaryCollection);

            if (DateTime.Now.Ticks % 2 == 0)
            {
                return $"{firstValue} {secondValue}";
            }

            return $"{secondValue} {firstValue}";
        }

        public string GetRandomValue(IList<string> primaryCollection, IList<string> secondaryCollection, IList<string> ternaryCollection)
        {
            var firstValue = GetRandomValue(primaryCollection, secondaryCollection);
            var secondValue = GetRandomValue(secondaryCollection);

            if (DateTime.Now.Ticks % 3 == 0)
            {
                return $"{GetRandomValue(primaryCollection, secondaryCollection)} {GetRandomValue(ternaryCollection)}";
            }

            return $"{GetRandomValue(primaryCollection, ternaryCollection)} {GetRandomValue(secondaryCollection)}";
        }

        public RandomValueWithComponents GetRandomValueWithComponents(IList<string> primaryCollection, IList<string> secondaryCollection)
        {
            var firstValue = GetRandomValue(primaryCollection);
            var secondValue = GetRandomValue(secondaryCollection);

            return new RandomValueWithComponents()
            {
                Components = new List<string>() { firstValue, secondValue },
                RandomValue = $"{firstValue} {secondValue}"
            };
        }


        #region Helpers

        //private static ReadOnlyCollection<string> createSampleCollection(string csvFileName)
        //{
        //    var reader = new StreamReader(File.OpenRead(@"Samples\" + csvFileName));
        //    List<string> listA = new List<string>();
        //    while (!reader.EndOfStream)
        //    {
        //        var line = reader.ReadLine();

        //        listA.Add(line);
        //    }

        //    return new ReadOnlyCollection<string>(listA);
        //}

        private static ReadOnlyCollection<string> createSampleCollection(string csvFileName)
        {
            List<string> listA = new List<string>();
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"SysKit.ODG.Samples.Samples.{csvFileName}";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    listA.Add(line);
                }
            }

            return new ReadOnlyCollection<string>(listA);
        }

        #endregion
    }
}
