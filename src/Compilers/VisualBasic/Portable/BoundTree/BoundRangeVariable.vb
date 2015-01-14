﻿' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.VisualBasic.Symbols
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.CodeAnalysis.VisualBasic

    Partial Class BoundRangeVariable
        Public Overrides ReadOnly Property ExpressionSymbol As Symbol
            Get
                Return Me.RangeVariable
            End Get
        End Property
    End Class

End Namespace
