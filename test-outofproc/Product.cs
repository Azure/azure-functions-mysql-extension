// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
using System.Collections.Generic;
using System;
using System.Linq;
using Google.Protobuf.WellKnownTypes;

namespace DotnetIsolatedTests.Common
{
    public class Product
    {
        public int? ProductId { get; set; }

        public string Name { get; set; }

        public int Cost { get; set; }
    }

    public class ProductIncorrectCasing
    {
        public int ProductID { get; set; }

        public string Name { get; set; }

        public int Cost { get; set; }
    }

    public class ProductUtilities
    {
        /// <summary>
        /// Returns a list of <paramref name="num"/> Products with sequential IDs, a cost of 100, and "test" as name.
        /// </summary>
        public static List<Product> GetNewProducts(int num)
        {
            var products = new List<Product>();
            for (int i = 0; i < num; i++)
            {
                var product = new Product
                {
                    ProductId = i,
                    Cost = 100 * i,
                    Name = "test"
                };
                products.Add(product);
            }
            return products;
        }

        /// <summary>
        /// Returns a list of <paramref name="num"/> Products with a random cost between 1 and <paramref name="cost"/>.
        /// Note that ProductId is randomized too so list may not be unique.
        /// </summary>
        public static List<Product> GetNewProductsRandomized(int num, int cost)
        {
            var r = new Random();

            var products = new List<Product>(num);
            for (int i = 0; i < num; i++)
            {
                var product = new Product
                {
                    ProductId = r.Next(1, num),
                    Cost = (int)Math.Round(r.NextDouble() * cost),
                    Name = "test"
                };
                products.Add(product);
            }
            return products;
        }
    }

    public class ProductColumnTypes
    {
        public int ProductId { get; set; }

        public sbyte TinyInt { get; set; }

        public short SmallInt { get; set; }

        public int MediumInt { get; set; }

        public int IntType { get; set; }

        public long BigInt { get; set; }

        public decimal DecimalType { get; set; }

        public decimal Numeric { get; set; }

        public double FloatType { get; set; }

        public double DoubleType { get; set; }

        public bool Bit { get; set; }

        public DateOnly Date { get; set; }

        public DateTime Datetime { get; set; }

        public Timestamp TimeStampType { get; set; }

        public TimeSpan Time { get; set; }

        public int Year { get; set; }

        public string CharType { get; set; }

        public string Varchar { get; set; }

        public byte[] Binary { get; set; }

        public byte[] VarBinary { get; set; }

        public string Text { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is ProductColumnTypes)
            {
                var that = obj as ProductColumnTypes;
                return this.ProductId == that.ProductId && this.TinyInt == that.TinyInt && this.SmallInt == that.SmallInt
                    && this.MediumInt == that.MediumInt && this.IntType == that.IntType && this.BigInt == that.BigInt
                    && this.DecimalType == that.DecimalType && this.Numeric == that.Numeric && this.FloatType == that.FloatType
                    && this.DoubleType == that.DoubleType && this.Bit == that.Bit && this.Date == that.Date && this.Datetime == that.Datetime
                    && this.TimeStampType == that.TimeStampType && this.Time == that.Time && this.Year == that.Year
                    && this.CharType == that.CharType && this.Varchar == that.Varchar && this.Text == that.Text
                    && this.Binary.SequenceEqual(that.Binary) && this.VarBinary.SequenceEqual(that.VarBinary);
            }
            return false;
        }
        public override int GetHashCode() { return 0; }
    }

    public class ProductExtraColumns
    {
        public int ProductId { get; set; }

        public string Name { get; set; }

        public int Cost { get; set; }

        public int ExtraInt { get; set; }

        public string ExtraString { get; set; }
    }

    public class ProductMissingColumns
    {
        public int ProductId { get; set; }

        public string Name { get; set; }
    }

    public class ProductUnsupportedTypes
    {
        public int ProductId { get; set; }

        public string TextCol { get; set; }

        public string NtextCol { get; set; }

        public byte[] ImageCol { get; set; }
    }

    public class ProductDefaultPKWithDifferentColumnOrder
    {
        public int Cost { get; set; }

        public string Name { get; set; }
    }
}