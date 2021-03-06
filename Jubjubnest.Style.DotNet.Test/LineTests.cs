﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using Jubjubnest.Style.DotNet;
using Jubjubnest.Style.DotNet.Test.Helpers;

namespace Jubjubnest.Style.DotNet.Test
{
	[TestClass]
	public class LineTests : CodeFixVerifier
	{

		[TestMethod]
		public void TestEmpty()
		{
			var test = @"";

			VerifyCSharpDiagnostic( test );
		}

		[TestMethod]
		public void TestLineWithSpaceIndent()
		{
            var code = Code.InMethod( "    foo();" );

			VerifyCSharpDiagnostic( code.Code, Warning( code, 0, 1, LineAnalyzer.IndentWithTabs ) );
		}

		[TestMethod]
		public void TestLineWithMixedIndent()
		{
            var code = Code.InMethod( "\t    foo();" );

			VerifyCSharpDiagnostic( code.Code, Warning( code, 0, 2, LineAnalyzer.IndentWithTabs ) );
		}

		[TestMethod]
		public void TestLineWithTrailingWhitespace()
		{
			var code = "namespace Foo {}  ";

			VerifyCSharpDiagnostic( code, Warning( 1, 17, LineAnalyzer.NoTrailingWhitespace ) );

			VerifyCSharpFix( new TestEnvironment(), code, code.Trim() );
		}

		[TestMethod]
		public void TestLineOver120WithSpaces()
		{
			var code = Code.InMethod( new string( ' ', 116 ) + "int foo;" );

			VerifyCSharpDiagnostic( code.Code,
					Warning( code, 0, 1, LineAnalyzer.IndentWithTabs ),
					Warning( code, 0, 121, LineAnalyzer.KeepLinesWithin120Characters ) );
		}

		[TestMethod]
		public void TestLineOver30Tabs()
		{
			var code = Code.InMethod( new string( '\t', 29 ) + "int foo;" );

			VerifyCSharpDiagnostic( code.Code, Warning( code, 0, 34, LineAnalyzer.KeepLinesWithin120Characters ) );
		}

		[TestMethod]
		public void TestContinuationLineWithSingleIndent()
		{
			var code = Code.InMethod( @"
				int foo =
					1;" );

			VerifyCSharpDiagnostic( code.Code, Warning( code, 2, 6, LineAnalyzer.DoubleTabContinuationIndent ) );
		}

		[TestMethod]
		public void TestMultipleUsingStatements()
		{
			var code = Code.InMethod( @"
				using( var x = Foo() )
				using( var y = Bar() )
				{
					Console.WriteLine( x, y );
				}" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestBracesNotAlone()
		{
			var code = Code.InMethod( @"
				if( foo ) {
				} else {
				}" );

			VerifyCSharpDiagnostic( code.Code,
					Warning( code, 1, 15, LineAnalyzer.BracesOnTheirOwnLine ),
					Warning( code, 2, 5, LineAnalyzer.BracesOnTheirOwnLine ),
					Warning( code, 2, 12, LineAnalyzer.BracesOnTheirOwnLine ) );
		}

		[TestMethod]
		public void TestCommentAfterClosingBrace()
		{
			var code = Code.InMethod( @"
				if( foo )
				{
				}  // end if" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestCloseBraceWithParenthesis()
		{
			var code = Code.InMethod( @"
				Foo( foo =>
				{
					foo.i = 2;
				} );" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestCloseBraceWithExtraParameters()
		{
			var code = Code.InMethod( @"
				Foo( foo =>
				{
					foo.i = 2;
				}, bar );" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestCloseBraceWithAdditionalInvocations()
		{
			var code = Code.InMethod( @"
				Foo( foo =>
				{
					foo.i = 2;
				} ).ToList();" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestTwoLinePropertyWithBracesOnSharedLines()
		{
			var code = Code.InClass( @"
				public string Foo {
					get; set; }" );

			VerifyCSharpDiagnostic( code.Code,
					Warning( code, 1, 23, LineAnalyzer.BracesOnTheirOwnLine ),
					Warning( code, 2, 16, LineAnalyzer.BracesOnTheirOwnLine ) );
		}

		[TestMethod]
		public void TestSingleLineAutomaticProperties()
		{
			var code = Code.InClass( @"public string Foo { get; set; }" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestSingleLinePropertyAccessors()
		{
			var code = Code.InClass( @"
				public string Foo
				{
					get { return foo; }
					set { foo = value; }
				}

				public string Bar { get; set; }" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestSingleLinePropertyAccessorsWithAttributes()
		{
			var code = Code.InClass( @"
				[Attribute]
				public string Foo
				{
					get { return foo; }
					set { foo = value; }
				}

				[Attribute]
				public string Bar { get; set; }" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestSingleLineAutomaticPropertiesWithDefaultValue()
		{
			var code = Code.InClass( @"public string Foo { get; set; } = """";" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestParametersOnSameLine()
		{
			var code = Code.InClass( @"
				public string Foo( string a, string b )
				{
				}" );

			VerifyCSharpDiagnostic( code.Code, Warning( code, 1, 34, LineAnalyzer.ParametersOnTheirOwnLines, "b" ) );
		}

		[TestMethod]
		public void TestParametersOnTheirOwnLine()
		{
			var code = Code.InClass( @"
				public string Foo(
					string a,
					string b
				)
				{
				}" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestParameterParenSharingParameterLine()
		{
			var code = Code.InClass( @"
				public string Foo(
					string a,
					string b )
				{
				}" );

			VerifyCSharpDiagnostic( code.Code, Warning( 8, 15, LineAnalyzer.ClosingParameterParenthesesOnTheirOwnLines ) );
		}

		[TestMethod]
		public void TestCodeWithUnixNewlines()
		{
			var code = Code.InMethod( "int a = 0;\nint b = 0;" );

			VerifyCSharpDiagnostic( code.Code, Warning( 7, 11, LineAnalyzer.UseWindowsLineEnding ) );
		}

		[TestMethod]
		public void TestCodeWithAnonymousBlocks()
		{
			var code = Code.InMethod( @"
				{
					int i = 0;
				}" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestDisallowBaseConstructorOtherLine()
		{
			var code = Code.InClass( @"
				public Test(
					int i
				)
					: base( i )
				{

				}" );

			VerifyCSharpDiagnostic( code.Code, Warning( code, 4, 6, LineAnalyzer.BaseConstructorCallSameLine ) );
		}

		[TestMethod]
		public void TestAllowBaseConstructorSameLine()
		{
			var code = Code.InClass( @"
				public Test(
					int i
				) : base( i )
				{

				}" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestAllowBaseConstructorOtherLineWhenOneParameter()
		{
			var code = Code.InClass( @"
				public Test( int i )
					: base( i )
				{

				}" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestAttributeParametersAreNotOnTheirOwnLines()
		{
			var code = Code.InClass( @"
				[PrincipalPermission( Role = ""Administrators"",
					Action = SecurityAction.Demand, Name = ""Something"" )]
				public Test( int i )
				{

				}" );

			VerifyCSharpDiagnostic( code.Code, Warning( code, 2, 38, LineAnalyzer.AttributesOnTheirOwnLines ) );
		}

		[TestMethod]
		public void TestAttributeParametersAreNotOnTheirOwnLines2()
		{
			var code = Code.InClass( @"
				[PrincipalPermission( Role = ""Administrators"", Action = SecurityAction.Demand,
					Name = ""Something"" )]
				public Test( int i )
				{

				}" );

			VerifyCSharpDiagnostic( code.Code, Warning( code, 2, 6, LineAnalyzer.AttributesOnTheirOwnLines ) );
		}


		[TestMethod]
		public void TestAttributeParametersAreOnSameLine()
		{
			var code = Code.InClass( @"
				[Obsolete( Message = ""This constructor is obsolete."", IsError = true )]
				public Test( int i )
				{

				}" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestAttributeParametersAreOnTheirOwnLines1()
		{
			var code = Code.InClass( @"
				[Obsolete(
					Message = ""This constructor is obsolete."",
					IsError = true )]
				public Test( int i )
				{

				}" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestAttributeParametersAreOnTheirOwnLines2()
		{
			var code = Code.InClass( @"
				[Obsolete( Message = ""This constructor is obsolete."",
					IsError = true )]
				public Test( int i )
				{

				}" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestInitializerBracketsAreOnTheirOwnLines()
		{
			var code = Code.InMethod( @"
				var ex = new Exception()
				{
					Source = ""Test"",
					HelpLink = ""Test""
				}" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestInitializerBracketsOnSameLine()
		{
			var code = Code.InMethod( @"
				var ex = new Exception() { Source = ""Test"", HelpLink = ""Test"" }" );

			VerifyCSharpDiagnostic( code.Code );
		}

		[TestMethod]
		public void TestInitializerEndingBracketNextLine()
		{
			var code = Code.InMethod( @"
				var ex = new Exception() {
						Source = ""Test"", HelpLink = ""Test"" }" );
			VerifyCSharpDiagnostic( code.Code, Warning( code, 2, 42, LineAnalyzer.InitializerListOwnLines ) );
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new LineCodeFixProvider();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
            return new LineAnalyzer();
		}
	}
}