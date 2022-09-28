Public Class HumanPlayer
    Inherits Player

    Public Sub New(username As String, wins As Integer, losses As Integer, highscore As Integer, Board As List(Of BoardPosition), PlayerColour As ConsoleColor)
        MyBase.New(username, wins, losses, highscore, Board, PlayerColour)
    End Sub

    Protected Overrides Function IsHuman() As Boolean
        Return True
    End Function


    Protected Overrides Function DecisionBuyProperty(Square As BoardProperty, Price As Integer) As Boolean

        Dim choice As String = ""

        Do Until choice = "Y" Or choice = "N"
            Console.WriteLine()
            Console.Write("Would you like to buy ")
            If Square.GetType() = GetType(BoardProperty) Then
                Dim Prop As BoardProperty = Square
                Dim Colour As String = Prop.Colour
                ColourCheck(Colour)
            End If
            Console.Write(Square.Name)
            Console.ResetColor()
            Console.WriteLine(" for £" & Price & "?")

            Console.WriteLine("You currently have £" & money & " and own " & NumberOfPropertiesOwned(Square.Colour) & " " & Square.Colour & " properties")
            Console.WriteLine("Enter Y to buy, N not to buy, or anything else to see the Title Deed for this property.")
            choice = Console.ReadLine().ToUpper
            If choice.ToUpper = "Y" Then
                Return True
            ElseIf choice.ToUpper = "N" Then
                Return False
            Else
                TitleDeeds(Square)
            End If
        Loop

        Return False

    End Function

    Protected Overrides Function DecisionBuyStation(Square As BoardStation) As Boolean

        Dim choice As String = ""

        Do Until choice = "Y" Or choice = "N"
            Console.WriteLine()
            Console.WriteLine("Would you like to buy " & Square.Name & " for £" & Square.PriceToBuy & "?")
            Console.WriteLine("You currently have £" & money & " and own " & NumberOfStationsOwned() & " stations.")
            Console.WriteLine("Enter Y to buy, N not to buy, or anything else to see the Title Deed for this station.")
            choice = Console.ReadLine().ToUpper
            If choice.ToUpper = "Y" Then
                Return True
            ElseIf choice.ToUpper = "N" Then
                Return False
            Else
                TitleDeeds(Square)
            End If
        Loop

        Return False

    End Function

    Protected Overrides Function DecisionBuyUtility(Square As BoardUtility) As Boolean

        Dim choice As String = ""

        Do Until choice = "Y" Or choice = "N"
            Console.WriteLine()
            Console.WriteLine("Would you like to buy " & Square.Name & " for £" & Square.PriceToBuy & "?")
            Console.WriteLine("You currently have £" & money & " and own " & NumberOfUtilitiesOwned() & " utilities.")
            Console.WriteLine("Enter Y to buy, N not to buy, or anything else to see the Title Deed for this utility.")
            choice = Console.ReadLine().ToUpper
            If choice.ToUpper = "Y" Then
                Return True
            ElseIf choice.ToUpper = "N" Then
                Return False
            Else
                TitleDeeds(Square)
            End If
        Loop

        Return False

    End Function

    Protected Overrides Function DecisionUseGetOutOfJailFreeCard(NumberOfGetOutOfJailCards As Integer) As Boolean

        Dim choice As String = ""
        Do Until choice = "Y" Or choice = "N"
            If NumberOfGetOutOfJailCards = 2 Then
                Console.WriteLine("You currently have 2 get out of jail free cards, would you like to use one?")
            Else
                Console.WriteLine("You currently have 1 get out of jail free card, would you like to use it?")
            End If
            Console.WriteLine("Enter Y for yes and N for no.")
            choice = Console.ReadLine().ToUpper
        Loop

        Return choice = "Y"

    End Function

    Protected Overrides Function DecisionPayToGetOutOfJail() As Boolean

        Dim choice2 As String = ""
        Do Until choice2 = "Y" Or choice2 = "N"
            Console.WriteLine("Do you want to pay £50 to get out of jail?")
            Console.WriteLine("You currently have £" & money)
            Console.WriteLine("Enter Y for yes and N for no.")
            choice2 = Console.ReadLine().ToUpper()
        Loop

        Return choice2 = "Y"

    End Function

    Protected Overrides Sub DecisionEndTurn()

        Dim choice As String = ""

        DisplayBoard()

        Do Until choice = "6" Or HasLeft = True
            Console.WriteLine()
            Console.WriteLine("What would you like to do?")
            Console.WriteLine("(current balance: £" & money & ")")
            Console.WriteLine("1. build on properties")
            Console.WriteLine("2. sell buildings and properties")
            Console.WriteLine("3. mortgage and unmortgage properties")
            Console.WriteLine("4. see what properties you currently own")
            Console.WriteLine("5. view current status of properties")
            Console.WriteLine("6. end your turn")
            Console.WriteLine("7. leave the game")
            Console.WriteLine("8. go to jail (test)")
            Console.WriteLine("(enter number)")
            choice = Console.ReadLine()
            Select Case choice
                Case "1"
                    DecisionBuyBuildings()
                Case "2"
                    DecisionSell(ListOfPlayers, 0)
                Case "3"
                    Mortgage()
                Case "4"
                    SeeOwnedProperties()
                Case "5"
                    SeeStatusOfProperties()
                Case "7"
                    Dim choice2 As String = ""
                    Console.WriteLine()
                    Console.WriteLine("Are you sure you want to leave the game?")
                    Do Until choice2 = "Y" Or choice2 = "N"
                        Console.WriteLine("Enter Y for yes and N for no")
                        choice2 = Console.ReadLine().ToUpper
                        If choice2 = "Y" Then
                            HasLeft = True
                            Exit Sub
                        ElseIf choice2 <> "N" Then
                            Console.WriteLine("Input invalid.")
                            Console.WriteLine()
                        End If
                    Loop
                Case "8"
                    Jail()
            End Select
        Loop

    End Sub

    Protected Overrides Sub DecisionBuyBuildings()

        Dim HouseBuildColours As New HashSet(Of String)
        Dim HotelBuildColours As New HashSet(Of String)
        Dim CanBuildHouse As Boolean = False
        Dim CanBuildHotel As Boolean = False
        Dim HouseBuildList As New List(Of BoardProperty)
        Dim HotelBuildList As New List(Of BoardProperty)

        GetHouseHotelBuildLists(HouseBuildList, HotelBuildList, HouseBuildColours, HotelBuildColours)

        CanBuildHouse = HouseBuildList.Count > 0
        CanBuildHotel = HotelBuildList.Count > 0

        If CanBuildHouse = False And CanBuildHotel = False Then
            Console.WriteLine("You do not own all properties of a colour, so you cannot build on any properties yet.")
            Console.ReadLine()
        Else
            Console.WriteLine()
            If CanBuildHouse = True And NumberOfHousesAvailable > 0 Then
                Console.WriteLine("You can currently build houses on: ")
                For Each colour In HouseBuildColours
                    ColourCheck(colour)
                    Console.WriteLine(colour & " Properties")
                    Console.ResetColor()
                Next
            ElseIf NumberOfHousesAvailable = 0 And CanBuildHouse = True Then
                CanBuildHouse = False
                Console.WriteLine()
                Console.WriteLine("There are no more available houses. You must wait till a house is sold back to the bank.")
            Else
                Console.WriteLine("You cannot build any houses at the moment.")
            End If
            Console.WriteLine()
            If CanBuildHotel = True And NumberOfHotelsAvailable > 0 Then
                Console.WriteLine("You can currently build hotels on: ")
                For Each colour In HotelBuildColours
                    ColourCheck(colour)
                    Console.WriteLine(colour & " Properties")
                    Console.ResetColor()
                Next
            ElseIf NumberOfHotelsAvailable = 0 And CanBuildHotel = True Then
                CanBuildHotel = False
                Console.WriteLine()
                Console.WriteLine("There are no more available hotels. You must wait till a hotel is sold back to the bank.")
            Else
                Console.WriteLine("You cannot build any hotels at the moment.")
            End If
        End If

        Console.WriteLine()

        If CanBuildHouse = True Or CanBuildHotel = True Then
            Console.Write("Press ")
            If CanBuildHouse Then
                Console.Write("1 to build houses, ")
            End If
            If CanBuildHotel Then
                Console.Write("2 to build hotels, ")
            End If
            Console.WriteLine("and anything else to exit.")
            Dim choice As String = Console.ReadLine()
            Select Case choice
                Case "1"
                    If CanBuildHouse Then
                        DecisionHouseBuy(HouseBuildList)
                    End If
                Case "2"
                    If CanBuildHotel Then
                        DecisionHotelBuy(HotelBuildList)
                    End If
            End Select
        End If

    End Sub

    Sub DecisionHouseBuy(HouseBuildList)

        Dim valid As Boolean = False
        Dim ChosenProperty As BoardProperty
        Dim PropertyName As String
        Dim choice As String = ""

        Console.WriteLine()
        Console.WriteLine("You can currently purchase houses on these properties:")
        For Each Prop In HouseBuildList
            Dim Colour As String = Prop.Colour
            ColourCheck(Colour)
            Console.WriteLine(Prop.Name.ToUpper)
            Console.ResetColor()
            Console.WriteLine("You currently have " & Prop.NumberOfHouses & " houses on this property")
            Console.WriteLine("The cost to build a house on this property is £" & Prop.BuildingCost)
        Next

        Do Until valid = True
            Console.WriteLine()
            Console.WriteLine("Enter the name of the property you want to build a house on, or enter 5 to exit")
            PropertyName = Console.ReadLine().ToUpper
            For Each Prop In HouseBuildList
                If PropertyName = Prop.Name.ToUpper Then
                    ChosenProperty = Prop
                    valid = True
                    Exit For
                ElseIf PropertyName = "5" Then
                    valid = True
                End If
            Next
            If valid = False Then
                Console.WriteLine("Input is invalid. ")
            End If
        Loop

        If valid = True And PropertyName <> "5" Then
            If money >= ChosenProperty.BuildingCost Then
                Do Until choice = "N" Or choice = "Y"
                    Console.WriteLine()
                    Console.WriteLine("You currently have £" & money)
                    Console.Write("Would you like to buy a house on ")
                    Dim Colour As String = ChosenProperty.Colour
                    ColourCheck(Colour)
                    Console.Write(ChosenProperty.Name)
                    Console.ResetColor()
                    Console.WriteLine(" for £" & ChosenProperty.BuildingCost)
                    Console.WriteLine("Enter Y for yes and N for no")
                    choice = Console.ReadLine().ToUpper
                    If choice = "Y" Then
                        HouseBuy(ChosenProperty)
                    End If
                Loop
            Else
                Console.WriteLine()
                Console.WriteLine("You do not have enough money to buy a house on this property.")
            End If
        End If

    End Sub

    Sub DecisionHotelBuy(HotelBuildList)

        Dim valid As Boolean = False
        Dim ChosenProperty As BoardProperty
        Dim PropertyName As String
        Dim choice As String = ""

        Console.WriteLine()
        Console.WriteLine("You can currently purchase hotels on these properties:")
        For Each Prop In HotelBuildList
            Dim Colour As String = Prop.Colour
            ColourCheck(Colour)
            Console.WriteLine(Prop.Name.ToUpper)
            Console.ResetColor()
            Console.WriteLine("The cost to build a hotel on this property is £" & Prop.BuildingCost & " plus 4 houses")
        Next

        Do Until valid = True
            Console.WriteLine()
            Console.WriteLine("Enter the name of the property you want to build a hotel on, or enter 5 to exit")
            PropertyName = Console.ReadLine().ToUpper
            For Each Prop In HotelBuildList
                If PropertyName = Prop.Name.ToUpper Then
                    ChosenProperty = Prop
                    valid = True
                    Exit For
                ElseIf PropertyName = "5" Then
                    valid = True
                End If
            Next
            If valid = False Then
                Console.WriteLine("Input is invalid. ")
            End If
        Loop

        If valid = True And PropertyName <> "5" Then
            If money >= ChosenProperty.BuildingCost Then
                Do Until choice = "N" Or choice = "Y"
                    Console.WriteLine()
                    Console.WriteLine("You currently have £" & money)
                    Console.Write("Would you like to buy a hotel on ")
                    Dim Colour As String = ChosenProperty.Colour
                    ColourCheck(Colour)
                    Console.Write(ChosenProperty.Name)
                    Console.ResetColor()
                    Console.WriteLine(" for £" & ChosenProperty.BuildingCost)
                    Console.WriteLine("Enter Y for yes and N for no")
                    choice = Console.ReadLine().ToUpper
                    If choice = "Y" Then
                        HotelBuy(ChosenProperty)
                    End If
                Loop
            Else
                Console.WriteLine()
                Console.WriteLine("You do not have enough money to buy a hotel on this property.")
            End If
        End If

    End Sub

    Protected Overrides Function DecisionSellBuildings() As Boolean

        Dim AreThereAnyBuildingsToSell As Boolean = False
        Dim ListOfProperties As New List(Of BoardProperty)

        Console.WriteLine()
        For Each Properties As BoardProperty In ListOfBoardProperties
            If Properties.Owner IsNot Nothing Then
                If Properties.Owner Is Me Then
                    If Properties.NumberOfHouses > 0 Or Properties.NumberOfHotels > 0 Then
                        AreThereAnyBuildingsToSell = True
                        ListOfProperties.Add(Properties)
                        Exit For
                    End If
                End If
            End If
        Next

        If AreThereAnyBuildingsToSell = True Then
            Console.WriteLine("You currently own these buildings on these properties:")
            For Each BuildingProperty As BoardProperty In ListOfBoardProperties
                Dim Colour As String = BuildingProperty.Colour
                ColourCheck(Colour)
                If BuildingProperty.NumberOfHouses > 0 Then
                    Console.Write(BuildingProperty.Name)
                    Console.ResetColor()
                    Console.WriteLine(" - " & BuildingProperty.NumberOfHouses & " houses")
                ElseIf BuildingProperty.NumberOfHotels > 0 Then
                    Console.Write(BuildingProperty.Name)
                    Console.ResetColor()
                    Console.WriteLine(" - " & BuildingProperty.NumberOfHotels & " hotel")
                End If
            Next
            Console.ResetColor()
            Dim valid As Boolean = False
            Dim PropertyName As String
            Do Until valid = True
                Console.WriteLine()
                Console.WriteLine("Enter the name of the property you want to sell buildings on, or enter 5 to exit.")
                PropertyName = Console.ReadLine()
                For Each BuildingProperty As BoardProperty In ListOfBoardProperties
                    If PropertyName.ToUpper = BuildingProperty.Name.ToUpper Then
                        Dim CanHouseBeSold As Boolean = True
                        Dim CanHotelBeSold As Boolean = True
                        For Each OtherProp As BoardProperty In ListOfBoardProperties
                            If BuildingProperty.Colour = OtherProp.Colour Then
                                If BuildingProperty.NumberOfHouses < OtherProp.NumberOfHouses Or BuildingProperty.NumberOfHouses = 0 Then
                                    CanHouseBeSold = False
                                End If
                                If BuildingProperty.NumberOfHotels = 0 Then
                                    CanHotelBeSold = False
                                End If
                            End If
                        Next
                        If CanHotelBeSold Or CanHouseBeSold Then
                            PropertyName = BuildingProperty.Name
                            valid = True
                            Dim cost As Integer
                            cost = BuildingProperty.BuildingCost / 2
                            Dim choice As String = ""
                            Do Until choice = "Y" Or choice = "N"
                                If BuildingProperty.NumberOfHouses > 0 Then
                                    Console.Write("Do you want to sell a house on ")
                                    Dim Colour As String = BuildingProperty.Colour
                                    ColourCheck(Colour)
                                    Console.Write(BuildingProperty.Name)
                                    Console.ResetColor()
                                    Console.WriteLine(" for £" & cost & "?")
                                    Console.WriteLine("Enter Y for yes and N for no.")
                                    choice = Console.ReadLine().ToUpper
                                    If choice = "Y" Then
                                        valid = True
                                        SellHouse(BuildingProperty)
                                    ElseIf choice = "N" Then
                                        valid = True
                                    End If
                                Else
                                    If NumberOfHousesAvailable < 4 Then
                                        Console.WriteLine("There are not enough available houses to replace your hotel. You must wait until enough houses are returned to the bank.")
                                        valid = True
                                        Exit For
                                    End If
                                    Console.Write("Do you want to sell a hotel on ")
                                    Dim Colour As String = BuildingProperty.Colour
                                    ColourCheck(Colour)
                                    Console.Write(BuildingProperty.Name)
                                    Console.ResetColor()
                                    Console.WriteLine(" for £" & cost & "?")
                                    Console.WriteLine("Enter Y for yes and N for no.")
                                    choice = Console.ReadLine().ToUpper
                                    If choice = "Y" Then
                                        valid = True
                                        SellHotel(BuildingProperty)
                                    ElseIf choice = "N" Then
                                        valid = True
                                    End If
                                End If
                            Loop
                        Else
                            Console.WriteLine()
                            Console.WriteLine("Buildings cannot be sold on this property right now, as buildings need to be sold evenly.")
                            valid = True
                            Exit For
                        End If
                    ElseIf PropertyName = "5" Then
                        valid = True
                    End If
                Next
                If valid = False Then
                    Console.WriteLine()
                    Console.WriteLine("Input invalid.")
                End If
            Loop
        Else
            Console.WriteLine("You do not have any buildings on any properties.")
        End If

        Return True

    End Function

    Protected Overrides Sub DecisionSell(ListOfPlayers As List(Of Player), MoneyRequired As Integer)

        Dim choice As String = ""
        If MoneyRequired > 0 Then
            Do Until choice = "Y" Or choice = "N"
                Console.WriteLine("Would you like to sell to try and earn enough money? You need to earn £" & MoneyRequired & " to pay off this debt.")
                Console.WriteLine("Enter Y for yes and N for no")
                Console.WriteLine("(entering N will mean you lose the game)")
                choice = Console.ReadLine().ToUpper
            Loop
            Console.WriteLine()
            If choice = "N" Then
                Exit Sub
            End If
        End If

        choice = ""

        Do Until choice = "3"
            Console.WriteLine()
            Console.WriteLine("Enter 1 to sell buildings, 2 to sell properties and 3 to exit.")
            choice = Console.ReadLine()

            Select Case choice
                Case "1"
                    DecisionSellBuildings()
                Case "2"
                    SellProperties(ListOfPlayers)
                Case <> "3"
                    Console.WriteLine("Input invaild.")
            End Select
        Loop

    End Sub

    Public Sub DisplayBoard()

        TopAndBottomRows(20, 30, "top")
        Dim Start As Integer = 19
        Dim Finish As Integer = 31
        Do Until Start = 10 And Finish = 40
            'for middle rows, start represents left column and finish represents right column
            MiddleRows(Start, Finish)
            Start -= 1
            Finish += 1
        Loop
        TopAndBottomRows(0, 10, "bottom")

    End Sub

    Sub TopAndBottomRows(Start As Integer, Finish As Integer, TopOrBottomRow As String)

        Console.ForegroundColor = ConsoleColor.Black
        Console.BackgroundColor = ConsoleColor.White
        Dim width As Integer

        For row As Integer = 1 To 6
            If TopOrBottomRow = "top" Then
                For i As Integer = Start To Finish
                    CreateRows(Start, Finish, width, i, row)
                Next
            Else
                For i As Integer = Finish To Start Step -1
                    CreateRows(Start, Finish, width, i, row)
                Next
            End If
            Console.WriteLine()
        Next

        Console.ForegroundColor = ConsoleColor.White

    End Sub

    Sub CreateRows(Start, Finish, width, i, row)

        Dim boardposition As BoardPosition = ListOfBoardPositions(i)
        If row = 1 Then
            width = 13
            If i = Start Or i = Finish Then
                width *= 2
            End If
            Console.Write("|")
            For j = 1 To width - 1
                If boardposition.GetType() = (GetType(BoardProperty)) Then
                    BoardColourCheck(boardposition)
                Else
                    Console.BackgroundColor = ConsoleColor.White
                End If
                Console.Write(" ")
                Console.BackgroundColor = ConsoleColor.White
            Next
        Else
            width = 13
            If i = Start Or i = Finish Then
                width *= 2
            End If
            Console.ForegroundColor = ConsoleColor.Black
            Console.BackgroundColor = ConsoleColor.White
            Console.Write("|")
            Dim text As String = ""
            If row = 2 Then
                text = ListOfBoardPositions(i).Row1Text
            ElseIf row = 3 Then
                text = ListOfBoardPositions(i).Row2Text
            ElseIf row = 4 Then
                If boardposition.GetType().IsSubclassOf(GetType(SellablePosition)) Then
                    Dim Sellable As SellablePosition = ListOfBoardPositions(i)
                    text = "£" & Sellable.PriceToBuy
                End If
            ElseIf row = 5 Then
                If boardposition.GetType() = GetType(BoardProperty) Then
                    Dim prop As BoardProperty = ListOfBoardPositions(i)
                    If prop.NumberOfHouses > 0 Then
                        text = prop.NumberOfHouses & " houses"
                    ElseIf prop.NumberOfHouses > 0 Then
                        text = prop.NumberOfHotels & " hotel"
                    End If
                End If
            ElseIf row = 6 Then
                'displays where each player is on the board
                Dim textlength As Integer = 0
                For Each player As Player In ListOfPlayers
                    If player.Position = boardposition.ID Then
                        Console.Write("_")
                        Console.BackgroundColor = player.PlayerColour
                        Console.Write("_")
                        Console.BackgroundColor = ConsoleColor.White
                        textlength += 2
                    End If
                Next
                For i = 0 To width - 2 - textlength
                    text &= "_"
                Next
                Console.Write(text.PadRight(width - 1 - textlength))
            End If

            If text.Length > width - 1 Then
                text = text.Substring(0, width - 1)
            End If
            If row <> 6 Then
                Console.BackgroundColor = ConsoleColor.White
                Console.Write(text.PadRight(width - 1))
            End If
            Console.BackgroundColor = ConsoleColor.Black
        End If

    End Sub

    Sub MiddleRows(Start, Finish)

        Console.ForegroundColor = ConsoleColor.Black
        Console.BackgroundColor = ConsoleColor.White
        Dim width As Integer = 13

        For row As Integer = 1 To 6
            'left column
            CreateRows(Start, Finish, width, Start, row)
            Console.BackgroundColor = ConsoleColor.Black
            For i = 0 To 116
                Console.Write(" ")
            Next
            'right column
            Console.BackgroundColor = ConsoleColor.White
            CreateRows(Start, Finish, width, Finish, row)
            Console.WriteLine()
        Next

        Console.ForegroundColor = ConsoleColor.White

    End Sub

    Function BoardColourCheck(Position)

        Dim Prop As BoardProperty = Position

        Select Case Prop.Colour
            Case "Brown"
                Console.BackgroundColor = ConsoleColor.DarkRed
            Case "Light Blue"
                Console.BackgroundColor = ConsoleColor.DarkCyan
            Case "Pink"
                Console.BackgroundColor = ConsoleColor.Magenta
            Case "Orange"
                Console.BackgroundColor = ConsoleColor.DarkYellow
            Case "Red"
                Console.BackgroundColor = ConsoleColor.Red
            Case "Yellow"
                Console.BackgroundColor = ConsoleColor.Yellow
            Case "Green"
                Console.BackgroundColor = ConsoleColor.DarkGreen
            Case "Dark Blue"
                Console.BackgroundColor = ConsoleColor.DarkBlue
        End Select

        Return Console.ForegroundColor

    End Function

End Class
