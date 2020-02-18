using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SearchRankChecker.Business.Interfaces;
using SearchRankChecker.Business.Services;

namespace SearchRankChecker.Tests
{
    [TestFixture]
    public class GoogleRankCalculatorTests
    {
        private const string LookupRegexConfig = "HttpClients:GoogleClient:LookupRegex";
        private Mock<IConfiguration> _mockConfig;
        private IRankCalculator _googleRankCalculatorService;
        private Mock<ILogger<GoogleRankCalculator>> _logger;

        [SetUp]
        public void Setup()
        {
            _mockConfig = new Mock<IConfiguration>();
            _logger = new Mock<ILogger<GoogleRankCalculator>>();

            _googleRankCalculatorService = new GoogleRankCalculator(_mockConfig.Object, _logger.Object);
        }

        [Test]
        public void Rank_String_Should_Be_Returned_With_Site_Rank()
        {
            _mockConfig.SetupGet(c => c[LookupRegexConfig])
                .Returns("(<div class=\"r\"><a href=\"(.*?)\">)");

            var searchResults =
                "<div class=\"r\"><a href=\"http://www.infotrack.com.au\">Test Dummy Data</a></div>";

            var ranks = _googleRankCalculatorService
                .GetUrlRanksFromSearchResults(searchResults, new Uri("http://www.infotrack.com.au"));

            Assert.That(ranks, Is.EqualTo("1"));
        }

        [Test]
        public void Rank_String_Should_Be_Comma_Separated_If_More_Than_One_Matches()
        {
            _mockConfig.SetupGet(c => c[LookupRegexConfig])
                .Returns("(<div class=\"r\"><a href=\"(.*?)\">)");

            var searchResults ="<div class=\"r\"><a href=\"http://www.infotrack.com.au\">Test Dummy Data</a></div>" +
                            "<div class=\"r\"><a href=\"http://www.xyz.com.au\">Test Dummy Data</a></div>" +
                            "<div class=\"r\"><a href=\"http://www.infotrack.com.au\">Test Dummy Data</a></div>";

            var ranks = _googleRankCalculatorService
                .GetUrlRanksFromSearchResults(searchResults, new Uri("http://www.infotrack.com.au"));

            Assert.That(ranks, Is.EqualTo("1,3"));
        }

        [Test]
        public void Rank_String_Should_Be_Empty_If_No_Match_Found()
        {
            _mockConfig.SetupGet(c => c[LookupRegexConfig])
                .Returns("(<div class=\"r\"><a href=\"(.*?)\">)");

            var searchResults =
                "<div class=\"r\"><a href=\"http://www.xyz.com.au\">Test Dummy Data</a></div>";

            var ranks = _googleRankCalculatorService
                .GetUrlRanksFromSearchResults(searchResults, new Uri("http://www.infotrack.com.au"));

            Assert.That(ranks, Is.EqualTo("0"));
        }

        [Test]
        public void Argument_Exception_Should_Be_Thrown_If_Lookup_Regex_Not_Found()
        {
            _mockConfig.SetupGet(c => c[LookupRegexConfig])
                .Returns("");

            var searchResults =
                "<div class=\"r\"><a href=\"http://www.xyz.com.au\">Test Dummy Data</a></div>";
            
            var lookupException = Assert.Throws<ArgumentException>(() => _googleRankCalculatorService
                .GetUrlRanksFromSearchResults(searchResults, new Uri("http://www.infotrack.com.au")));

            Assert.That(lookupException.Message, Is.EqualTo("Lookup up regex not found!"));
        }

        [Test]
        public void Argument_Exception_Should_Be_Thrown_If_Search_Results_Not_Provided()
        {
            _mockConfig.SetupGet(c => c[LookupRegexConfig])
                .Returns("(<div class=\"r\"><a href=\"(.*?)\">)");

            var searchResults = "";

            var ranks = _googleRankCalculatorService.GetUrlRanksFromSearchResults(searchResults,
                new Uri("http://www.infotrack.com.au"));

            Assert.That(ranks, Is.EqualTo("0"));
        }
    }
}
