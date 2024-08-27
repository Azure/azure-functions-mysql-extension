// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace DotnetIsolatedTests.Common
{
    public class Product
    {
        public int? ProductId { get; set; }

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

        public long BigIntType { get; set; }

        public int BitType { get; set; }

        public decimal DecimalType { get; set; }

        public decimal NumericType { get; set; }

        public short SmallIntType { get; set; }

        public short TinyIntType { get; set; }

        public double FloatType { get; set; }

        public float RealType { get; set; }

        public string DateType { get; set; }

        public string DatetimeType { get; set; }

        public TimeSpan TimeType { get; set; }

        public string CharType { get; set; }

        public string VarcharType { get; set; }

        public string NcharType { get; set; }

        public string NvarcharType { get; set; }

        public byte[] BinaryType { get; set; }

        public byte[] VarbinaryType { get; set; }


        public override bool Equals(object obj)
        {
            if (obj is ProductColumnTypes)
            {
                var that = obj as ProductColumnTypes;
                Console.WriteLine("Debug Values: " + $"{this.ProductId == that.ProductId} {this.BigIntType == that.BigIntType} {this.BitType == that.BitType} {this.DecimalType == that.DecimalType} {this.NumericType == that.NumericType} {this.SmallIntType == that.SmallIntType} {this.TinyIntType == that.TinyIntType} {this.FloatType == that.FloatType} {this.RealType == that.RealType} {this.DateType == that.DateType} {this.DatetimeType == that.DatetimeType} {this.TimeType == that.TimeType} {this.CharType == that.CharType} {this.VarcharType == that.VarcharType} {this.NcharType == that.NcharType} {this.NvarcharType == that.NvarcharType} {this.BinaryType.SequenceEqual(that.BinaryType)} {this.VarbinaryType.SequenceEqual(that.VarbinaryType)}");
                Console.WriteLine("This date: " + this.DatetimeType);
                Console.WriteLine("That date: " + that.DatetimeType);
                Console.WriteLine("This binary: " + this.BinaryType);
                Console.WriteLine("That binary: " + that.BinaryType);
                return this.ProductId == that.ProductId && this.BigIntType == that.BigIntType && this.BitType == that.BitType &&
                    this.DecimalType == that.DecimalType && this.NumericType == that.NumericType &&
                    this.SmallIntType == that.SmallIntType && this.TinyIntType == that.TinyIntType &&
                    this.FloatType == that.FloatType && this.RealType == that.RealType && this.DateType == that.DateType &&
                    this.DatetimeType == that.DatetimeType && this.TimeType == that.TimeType && this.CharType == that.CharType &&
                    this.VarcharType == that.VarcharType && this.NcharType == that.NcharType && this.NvarcharType == that.NvarcharType &&
                    this.BinaryType.SequenceEqual(that.BinaryType) && this.VarbinaryType.SequenceEqual(that.VarbinaryType);
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

    public class ProductDefaultPKWithDifferentColumnOrder
    {
        public int Cost { get; set; }

        public string Name { get; set; }
    }
}