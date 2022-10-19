﻿using T = Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;  //need to install pkg
using SDC.Schema;
using System;

//This file was autogenerated

namespace SDCObjectModelTests.UtilityClasses
{
	[TestClass]
public class HexConversionsTests
{
    private MockRepository mockRepository;



    [TestInitialize]
    public void TestInitialize()
    {
        this.mockRepository = new MockRepository(MockBehavior.Strict);


    }

    private HexConversions CreateHexConversions()
    {
        return new HexConversions();
            
    }

    [TestMethod]
    public void HexStringToByteArrayFastest_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var hexConversions = this.CreateHexConversions();
        string hex = null;

        // Act
        var result = hexConversions.HexStringToByteArrayFastest(
            hex);

        // Assert
        Assert.Fail();
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    public void HexStringToByteArraySlow_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var hexConversions = this.CreateHexConversions();
        string hex = null;

        // Act
        var result = hexConversions.HexStringToByteArraySlow(
            hex);

        // Assert
        Assert.Fail();
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    public void HexStringToBytes_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var hexConversions = this.CreateHexConversions();
        string hexString = null;

        // Act
        var result = hexConversions.HexStringToBytes(
            hexString);

        // Assert
        Assert.Fail();
        this.mockRepository.VerifyAll();
    }
}
}