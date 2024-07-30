/**
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for
 * license information.
 */

package com.function.Common;

import java.math.BigDecimal;

public class ProductColumnTypes {
    private int ProductId;
    private long BigInt;
    private boolean Bit;
    private BigDecimal DecimalType;
    private BigDecimal Numeric;
    private short SmallInt;
    private short TinyInt;
    private double FloatType;
    private double Real;
    private String Date;
    private String Datetime;
    private String Time;
    private String CharType;
    private String Varchar;
    private String Nchar;
    private String Nvarchar;
    private String Binary;
    private String Varbinary;


    public ProductColumnTypes(int productId, long bigInt, boolean bit, BigDecimal decimalType, 
    BigDecimal numeric, short smallInt, short tinyInt, double floatType, double real, String date,
    String datetime, String time, String charType,
    String varchar, String nchar, String nvarchar, String binary, String varbinary) {
        ProductId = productId;
        BigInt = bigInt;
        Bit = bit;
        DecimalType = decimalType;
        Numeric = numeric;
        SmallInt = smallInt;
        TinyInt = tinyInt;
        FloatType = floatType;
        Real = real;
        Date = date;
        Datetime = datetime;
        Time = time;
        CharType = charType;
        Varchar = varchar;
        Nchar = nchar;
        Nvarchar = nvarchar;
        Binary = binary;
        Varbinary = varbinary;
    }

    public int getProductId() {
        return ProductId;
    }

    public long getBigint() {
        return BigInt;
    }

    public void setBigint(long bigInt) {
        BigInt = bigInt;
    }

    public boolean getBit() {
        return Bit;
    }

    public void setBit(boolean bit) {
        Bit = bit;
    }

    public BigDecimal getDecimalType() {
        return DecimalType;
    }

    public void setDecimalType(BigDecimal decimalType) {
        DecimalType = decimalType;
    }

    public BigDecimal getNumeric() {
        return Numeric;
    }

    public void setNumeric(BigDecimal numeric) {
        Numeric = numeric;
    }

    public short getSmallInt() {
        return SmallInt;
    }

    public void setSmallInt(short smallInt) {
        SmallInt = smallInt;
    }

    public short getTinyInt() {
        return TinyInt;
    }

    public void setTinyInt(short tinyInt) {
        TinyInt = tinyInt;
    }

    public double getFloatType() {
        return FloatType;
    }

    public void setFloatType(double floatType) {
        FloatType = floatType;
    }

    public double getReal() {
        return Real;
    }

    public void setReal(double real) {
        Real = real;
    }

    public String getDate() {
        return Date;
    }

    public void setDate(String date) {
        Date = date;
    }

    public String getDatetime() {
        return Datetime;
    }

    public void setDatetime(String datetime) {
        Datetime = datetime;
    }

    public String getTime() {
        return Time;
    }

    public void setTime(String time) {
        Time = time;
    }

    public String getCharType() {
        return CharType;
    }

    public void setCharType(String charType) {
        CharType = charType;
    }

    public String getVarchar() {
        return Varchar;
    }

    public void setVarchar(String varchar) {
        Varchar = varchar;
    }

    public String getNchar() {
        return Nchar;
    }

    public void setNchar(String nchar) {
        Nchar = nchar;
    }

    public String getNvarchar() {
        return Nvarchar;
    }

    public void setNvarchar(String nvarchar) {
        Nvarchar = nvarchar;
    }

    public String getBinary() {
        return Binary;
    }

    public void setBinary(String binary) {
        Binary = binary;
    }

    public String getVarbinary() {
        return Varbinary;
    }

    public void setVarbinary(String varbinary) {
        Varbinary = varbinary;
    }
}