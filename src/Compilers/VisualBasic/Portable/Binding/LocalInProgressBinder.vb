﻿' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Collections.Generic
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.VisualBasic.Symbols
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.CodeAnalysis.VisualBasic

    ''' <summary>
    ''' This binder is for binding the initializer of an implicitly typed 
    ''' local variable. While binding an implicitly typed local variable
    ''' this binder is used to break cycles.
    ''' </summary>
    Friend NotInheritable Class LocalInProgressBinder
        Inherits Binder

        ' In certain scenarios we might find ourselves in loops, like
        ' 
        ' dim x = y
        ' dim y = M(x)
        '
        ' We break the cycle by ensuring that an initializer which illegally refers
        ' forwards to an in-scope local does not attempt to work out the type of the
        ' forward local. However, just to make sure, we also keep track of every
        ' local whose type we are attempting to infer. (This might be necessary for
        ' "script class" scenarios where local vars are actually fields.)
        Private symbols As ConsList(Of LocalSymbol)

        Public Sub New(containingBinder As Binder, symbol As LocalSymbol)
            MyBase.New(containingBinder)
            Me.symbols = New ConsList(Of LocalSymbol)(symbol, containingBinder.ImplicitlyTypedLocalsBeingBound)
        End Sub

        Public Overrides ReadOnly Property ImplicitlyTypedLocalsBeingBound As ConsList(Of LocalSymbol)
            Get
                Return Me.symbols
            End Get

        End Property

    End Class

End Namespace

