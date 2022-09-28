Imports System.Data.OleDb

Public Class BoardPosition

    Public ID As Integer
    Public Name As String
    Public Row1Text As String
    Public Row2Text As String

    Public Sub New(ID As Integer, Name As String, Row1Text As String, Row2Text As String)

        Me.ID = ID
        Me.Name = Name
        Me.Row1Text = Row1Text
        Me.Row2Text = Row2Text

    End Sub

End Class

Public MustInherit Class SellablePosition
    Inherits BoardPosition

    Public Owner As Player = Nothing
    Public IsMortgaged As Boolean = False
    Public PriceToBuy, MortgageValue As Integer

    Sub New(ID As Integer, Name As String, Row1Text As String, Row2Text As String)

        MyBase.New(ID, Name, Row1Text, Row2Text)

    End Sub

    Public Overridable Function CanBeMortgaged(player As Player) As Boolean
        Return Owner Is player
    End Function

    Public MustOverride Function GetRent() As Integer

End Class

Public Class BoardProperty
    Inherits SellablePosition

    Public NumberOfProperties, Rent, House1Rent, House2Rent, House3Rent, House4Rent, HotelRent, BuildingCost As Integer
    Public Colour As String
    Public NumberOfHouses As Integer = 0
    Public NumberOfHotels As Integer = 0

    Public Sub New(ID As Integer, Name As String, Row1Text As String, Row2Text As String)

        MyBase.New(ID, Name, Row1Text, Row2Text)

        Dim searchString As String = "Select * From [Property] Where [ID] = @ID"

        Try
            'assign the database we established at the top of the code to the connection string element of conn
            Dim conn As New OleDbConnection
            conn.ConnectionString = connString2
            'Open connection to the database
            conn.Open()

            'Cmd will represent the connection and sql statement that we wish to run on the database
            Dim cmd As New OleDbCommand(searchString, conn)
            Dim dap As New OleDbDataAdapter
            dap.SelectCommand = cmd

            cmd.Parameters.AddWithValue("@ID", ID)

            'using the data adapter, run the query and fill the datatable with the returned data results
            Dim dt As New DataTable
            dap.Fill(dt)

            'close the connection to the database
            conn.Close()

            Dim row As DataRow = dt.Rows(0)
            PriceToBuy = row("Price")
            Colour = row("Colour")
            NumberOfProperties = row("NumberOfProperties")
            Rent = row("Rent")
            House1Rent = row("1HouseRent")
            House2Rent = row("2HouseRent")
            House3Rent = row("3HouseRent")
            House4Rent = row("4HouseRent")
            HotelRent = row("HotelRent")
            BuildingCost = row("BuildingCost")
            MortgageValue = PriceToBuy / 2

            dt.Clear()

            'catch any errors in the process and output them if necessary
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

    End Sub

    Public Overrides Function CanBeMortgaged(player As Player) As Boolean
        Return Owner Is player And NumberOfHouses = 0 And NumberOfHotels = 0
    End Function

    Public Overrides Function GetRent() As Integer

        Dim PriceToPay As Integer = 0

        If IsMortgaged = False Then
            If Owner.DoesOtherPlayerOwnAllProperties(Colour, Owner) Then
                If NumberOfHotels = 0 And NumberOfHouses = 0 Then
                    For Each prop In ListOfBoardProperties
                        If prop.Colour = Colour And prop.IsMortgaged = True Then
                            Return Rent
                        End If
                    Next
                    PriceToPay = Rent * 2
                ElseIf NumberOfHouses = 1 Then
                    PriceToPay = House1Rent
                ElseIf NumberOfHouses = 2 Then
                    PriceToPay = House2Rent
                ElseIf NumberOfHouses = 3 Then
                    PriceToPay = House3Rent
                ElseIf NumberOfHouses = 4 Then
                    PriceToPay = House4Rent
                Else
                    PriceToPay = HotelRent
                End If
            Else
                PriceToPay = Rent
            End If
        End If

        Return PriceToPay

    End Function

End Class

Public Class BoardUtility
    Inherits SellablePosition


    Public Multiplier1, Multiplier2 As Integer

    Public Sub New(ID As Integer, Name As String, Row1Text As String, Row2Text As String)

        MyBase.New(ID, Name, Row1Text, Row2Text)

        Dim searchString As String = "Select * From [Utility] Where [ID] = @ID"

        Try
            'assign the database we established at the top of the code to the connection string element of conn
            Dim conn As New OleDbConnection
            conn.ConnectionString = connString2
            'Open connection to the database
            conn.Open()

            'Cmd will represent the connection and sql statement that we wish to run on the database
            Dim cmd As New OleDbCommand(searchString, conn)
            Dim dap As New OleDbDataAdapter
            dap.SelectCommand = cmd

            cmd.Parameters.AddWithValue("@ID", ID)

            'using the data adapter, run the query and fill the datatable with the returned data results
            Dim dt As New DataTable
            dap.Fill(dt)

            'close the connection to the database
            conn.Close()

            Dim row As DataRow = dt.Rows(0)
            PriceToBuy = row("Price")
            Multiplier1 = row("1Multiplier")
            Multiplier2 = row("2Multiplier")
            MortgageValue = PriceToBuy / 2

            dt.Clear()

            'catch any errors in the process and output them if necessary
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

    End Sub

    Public Overrides Function GetRent() As Integer

        Dim Number As Integer = Owner.NumberOfUtilitiesOwned()
        If Number = 1 Then
            Return Multiplier1
        Else
            Return Multiplier2
        End If
    End Function

End Class

Public Class BoardStation
    Inherits SellablePosition

    Public Rent, Station2Rent, Station3Rent, Station4Rent As Integer

    Public Sub New(ID As Integer, Name As String, Row1Text As String, Row2Text As String)

        MyBase.New(ID, Name, Row1Text, Row2Text)

        Dim searchString As String = "Select * From [Station] Where [ID] = @ID"

        Try
            'assign the database we established at the top of the code to the connection string element of conn
            Dim conn As New OleDbConnection
            conn.ConnectionString = connString2
            'Open connection to the database
            conn.Open()

            'Cmd will represent the connection and sql statement that we wish to run on the database
            Dim cmd As New OleDbCommand(searchString, conn)
            Dim dap As New OleDbDataAdapter
            dap.SelectCommand = cmd

            cmd.Parameters.AddWithValue("@ID", ID)

            'using the data adapter, run the query and fill the datatable with the returned data results
            Dim dt As New DataTable
            dap.Fill(dt)

            'close the connection to the database
            conn.Close()

            Dim row As DataRow = dt.Rows(0)
            PriceToBuy = row("Price")
            Rent = row("Rent")
            Station2Rent = row("2StationRent")
            Station3Rent = row("3StationRent")
            Station4Rent = row("4StationRent")
            MortgageValue = PriceToBuy / 2

            dt.Clear()

            'catch any errors in the process and output them if necessary
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

    End Sub

    Public Overrides Function GetRent() As Integer

        If IsMortgaged = False Then
            Dim Number As Integer = Owner.NumberOfStationsOwned()
            If Number = 1 Then
                Return Rent
            ElseIf Number = 2 Then
                Return Station2Rent
            ElseIf Number = 3 Then
                Return Station3Rent
            Else
                Return Station4Rent
            End If
        Else
            Return 0
        End If

    End Function

End Class

Public Class BoardChest
    Inherits BoardPosition

    Public Sub New(ID As Integer, Name As String, Row1Text As String, Row2Text As String)

        MyBase.New(ID, Name, Row1Text, Row2Text)

    End Sub

End Class

Public Class BoardChance
    Inherits BoardPosition

    Public Sub New(ID As Integer, Name As String, Row1Text As String, Row2Text As String)

        MyBase.New(ID, Name, Row1Text, Row2Text)

    End Sub

End Class

Public Class BoardOther
    Inherits BoardPosition

    Public Sub New(ID As Integer, Name As String, Row1Text As String, Row2Text As String)

        MyBase.New(ID, Name, Row1Text, Row2Text)

    End Sub

End Class

Public Class BoardGoToJail
    Inherits BoardPosition

    Public Sub New(ID As Integer, Name As String, Row1Text As String, Row2Text As String)

        MyBase.New(ID, Name, Row1Text, Row2Text)

    End Sub

End Class

Public Class BoardTax
    Inherits BoardPosition

    Public PriceToPay

    Public Sub New(ID As Integer, Name As String, Row1Text As String, Row2Text As String)

        MyBase.New(ID, Name, Row1Text, Row2Text)

        Dim searchString As String = "Select * From [Tax] Where [ID] = @ID"

        Try
            'assign the database we established at the top of the code to the connection string element of conn
            Dim conn As New OleDbConnection
            conn.ConnectionString = connString2
            'Open connection to the database
            conn.Open()

            'Cmd will represent the connection and sql statement that we wish to run on the database
            Dim cmd As New OleDbCommand(searchString, conn)
            Dim dap As New OleDbDataAdapter
            dap.SelectCommand = cmd

            cmd.Parameters.AddWithValue("@ID", ID)

            'using the data adapter, run the query and fill the datatable with the returned data results
            Dim dt As New DataTable
            dap.Fill(dt)

            'close the connection to the database
            conn.Close()

            Dim row As DataRow = dt.Rows(0)
            PriceToPay = row("Price")

            dt.Clear()

            'catch any errors in the process and output them if necessary
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

    End Sub

End Class

