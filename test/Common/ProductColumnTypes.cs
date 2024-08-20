// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.MySql.Tests.Common
{
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

        public DateTime DateType { get; set; }

        public DateTime DatetimeType { get; set; }

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

        public override string ToString()
        {
            return $"[{this.ProductId}, {this.BigIntType}, {this.BitType}, {this.DecimalType}, {this.NumericType}, {this.SmallIntType}, {this.TinyIntType}, {this.FloatType}, {this.RealType}, {this.DateType}, {this.DatetimeType}, {this.TimeType}, {this.CharType}, {this.VarcharType}, {this.NcharType}, {this.NvarcharType}, {this.BinaryType}, {this.VarbinaryType}]";
        }
    }
}
