Imports System.Data.OleDb

Public MustInherit Class Player

    'MAKE ATTRIBUTES PRIVATE AND USE GETTERS AND SETTERS
    Public Position As Integer
    Public money As Integer
    Shared ChanceCard As Integer = 0
    Shared CommunityChestCard As Integer = 0
    Dim doubles As Integer = 0
    Public username As String
    Public wins, losses, highscore As Integer
    Dim InJail As Boolean
    Dim JailTurnsRemaining As Integer
    Public DoesPlayerHaveChanceGetOutOfJailCard As Boolean = False
    Public DoesPlayerHaveCommunityChestGetOutOfJailCard As Boolean = False
    Public Shared ChanceCards As Queue(Of Integer)
    Public Shared CommunityChestCards As Queue(Of Integer)
    Public Board As List(Of BoardPosition)
    Public HasLeft As Boolean = False
    Public IsBankrupt As Boolean = False
    Public Shared NumberOfHousesAvailable As Integer = 32
    Public Shared NumberOfHotelsAvailable As Integer = 12
    Public PlayerColour As ConsoleColor


    Public Sub New(username As String, wins As Integer, losses As Integer, highscore As Integer, Board As List(Of BoardPosition), PlayerColour As ConsoleColor)

        Me.username = username
        Me.Board = ListOfBoardPositions
        Me.wins = wins
        Me.losses = losses
        Me.highscore = highscore
        Me.PlayerColour = PlayerColour
        Position = 0
        money = 1500
        InJail = False

    End Sub

    Public Function ChangeMoney(Amount As Integer, IsCurrentPlayer As Boolean, OwedPlayer As Player) As Boolean

        If (money + Amount) < 0 Then
            Dim MoneyRequired As Integer = 0 - (money + Amount)
            Console.WriteLine()
            Console.BackgroundColor = PlayerColour
            Console.Write(username)
            Console.BackgroundColor = ConsoleColor.Black
            Console.WriteLine(", you do not have enough money to pay this.")

            'add option to mortgage properties
            DecisionSell(ListOfPlayers, Math.Abs(Amount))

            If money >= Math.Abs(Amount) Then
                ChangeMoney(Amount, IsCurrentPlayer, OwedPlayer)
                Return True
            Else
                Console.BackgroundColor = PlayerColour
                Console.Write(username)
                Console.BackgroundColor = ConsoleColor.Black
                Console.WriteLine(" is bankrupt.")
                HasLeft = True
                BankruptPropertyUpdate(OwedPlayer)
            End If
            Return False
        Else
            money += Amount
            If IsCurrentPlayer = True And IsHuman() Then
                Console.WriteLine("You now have £" & money)
            Else
                Console.BackgroundColor = PlayerColour
                Console.Write(username)
                Console.BackgroundColor = ConsoleColor.Black
                Console.WriteLine(" now has £" & money)
            End If
            Return True
        End If

    End Function

    Public Sub TakeTurn(IsBonusTurn As Boolean, ListOfPlayers As List(Of Player))

        If HasLeft Then
            Exit Sub
        End If

        Dim PreviouslyInJail As Boolean = False

        Console.WriteLine()
        If IsBonusTurn = True And InJail = False Then
            Console.BackgroundColor = PlayerColour
            Console.Write(username)
            Console.BackgroundColor = ConsoleColor.Black
            Console.WriteLine("'s bonus turn.")
        Else
            Console.BackgroundColor = PlayerColour
            Console.Write(username)
            Console.BackgroundColor = ConsoleColor.Black
            Console.WriteLine("'s turn.")
        End If

        If InJail = True Then
            PreviouslyInJail = True
            JailTurnsRemaining = JailTurnsRemaining - 1
            If JailTurnsRemaining > 0 Then
                GetOutOfJail()
            Else
                InJail = False
                Console.WriteLine()
                Console.WriteLine("Your jail sentence has finished, you can now leave jail.")
            End If
        End If

        Dim DiceResult = RollDice()

        If InJail = True And doubles > 0 Then
            InJail = False
            Console.WriteLine("You have rolled a double, you are no longer in jail.")
        End If

        If InJail = False Then
            Position = Move(DiceResult.SpacesMoved, Board)
        End If

        DecisionEndTurn()

        If DiceResult.wasDouble And doubles <> 3 And PreviouslyInJail = False And InJail = False Then
            TakeTurn(True, ListOfPlayers)
        End If

    End Sub

    Public Sub AccountManagement()

        Dim choice As String

        Console.WriteLine("Press 1 to change your password, 2 to delete your account, or anything else to return to the main menu.")
        choice = Console.ReadLine()
        If choice = "1" Then
            ChangePassword()
        ElseIf choice = "2" Then
            DeleteAccount()
        Else
            MainMenu()
        End If

    End Sub

    Sub ChangePassword()

        Dim NewPassword, PasswordCheck, answer As String
        Dim confirmed As Boolean = False
        Dim PasswordLength As Boolean = False

        Do Until confirmed = True And PasswordLength = True
            Console.WriteLine("Enter new password")
            NewPassword = Console.ReadLine()
            If NewPassword.Length < 5 Then
                Console.WriteLine("Password is too short. It must be a minimum of 5 characters.")
            Else
                PasswordLength = True
                Console.WriteLine("Confirm password")
                PasswordCheck = Console.ReadLine()

                If NewPassword <> PasswordCheck Then
                    Console.WriteLine("Passwords do not match.")
                    Console.WriteLine("Press 1 to try again, or anything else to return to the main menu.")
                    answer = Console.ReadLine()
                    If answer <> "1" Then
                        confirmed = True
                        MainMenu()
                    Else
                    End If
                Else

                    Dim searchString As String = "Select * From Logins"

                    Try
                        'assign the database we established at the top of the code to the connection string element of conn
                        Dim conn As New OleDbConnection
                        conn.ConnectionString = connString
                        'Open connection to the database
                        conn.Open()

                        'Cmd will represent the connection and sql statement that we wish to run on the database 
                        Dim cmd As New OleDbCommand(searchString, conn)
                        Dim dap As New OleDbDataAdapter
                        dap.SelectCommand = cmd

                        'using the data adapter, run the query and fill the datatable with the returned data results
                        Dim dt As New DataTable
                        dap.Fill(dt)

                        Dim query As String = "UPDATE Logins SET  [Password]  = @NewPassword WHERE [Username] = @username "

                        'Cmd will represent the connection and sql statement that we wish to run on the database

                        Dim cmd2 As New OleDbCommand(query, conn)

                        cmd2.Parameters.AddWithValue("@Password", Module1.HashPassword(NewPassword).ToString)
                        cmd2.Parameters.AddWithValue("@Username", username)

                        'runs the command to upload details to the database table, this is different to a select command as we don’t expect a response

                        cmd2.ExecuteNonQuery()

                        'close the connection to the database
                        conn.Close()

                        'catch any errors in the process and output them if necessary
                    Catch ex As Exception
                        Console.WriteLine(ex.Message)
                        Return
                    End Try

                    Console.WriteLine("Your password has successfully been changed.")
                    Console.ReadLine()

                    MainMenu()

                End If
            End If
        Loop
    End Sub

    Sub DeleteAccount()

        Dim choice As String

        Console.WriteLine("Are you sure you want to delete your account?")
        Console.WriteLine("All of your data will be permanently lost.")
        Console.WriteLine("Press 1 to delete your account, or anything else to return to the main menu.")
        choice = Console.ReadLine()
        If choice = "1" Then

            Dim searchString As String = "Select * From Logins"

            Try
                'assign the database we established at the top of the code to the connection string element of conn
                Dim conn As New OleDbConnection
                conn.ConnectionString = connString
                'Open connection to the database
                conn.Open()

                'Cmd will represent the connection and sql statement that we wish to run on the database 
                Dim cmd As New OleDbCommand(searchString, conn)
                Dim dap As New OleDbDataAdapter
                dap.SelectCommand = cmd

                'using the data adapter, run the query and fill the datatable with the returned data results
                Dim dt As New DataTable
                dap.Fill(dt)

                Dim query = "DELETE * FROM Logins WHERE Username = @username"

                'Cmd will represent the connection and sql statement that we wish to run on the database

                Dim cmd2 As New OleDbCommand(query, conn)

                cmd2.Parameters.AddWithValue("@Username", username)

                'runs the command to upload details to the database table, this is different to a select command as we don’t expect a response

                cmd2.ExecuteNonQuery()

                'close the connection to the database
                conn.Close()

                'catch any errors in the process and output them if necessary
            Catch ex As Exception
                Console.WriteLine(ex.Message)
                Return
            End Try

            ListOfPlayers.Clear()
            Console.WriteLine("Account deleted.")
            Console.ReadLine()
            Main()
        Else
            MainMenu()
        End If
    End Sub



    Function RollDice() As (SpacesMoved As Integer, wasDouble As Boolean)

        Dim dice1, dice2, SpacesMoved As Integer

        Randomize()
        Console.WriteLine("Press enter to roll the dice.")
        If IsHuman() Then
            Console.ReadLine()
        End If
        dice1 = Int((6 * Rnd()) + 1)
        Console.WriteLine("Dice1: " & dice1)
        dice2 = Int((6 * Rnd()) + 1)
        Console.WriteLine("Dice2: " & dice2)
        Console.WriteLine()
        SpacesMoved = dice1 + dice2
        If dice1 = dice2 Then
            doubles += 1
            If doubles = 3 Then
                If IsHuman() Then
                    Console.WriteLine("You have rolled a double 3 times in a row. You must go directly to jail.")
                Else
                    Console.BackgroundColor = PlayerColour
                    Console.Write(username)
                    Console.BackgroundColor = ConsoleColor.Black
                    Console.WriteLine(" has rolled a double 3 times in a row so must go directly to jail.")
                End If
                SpacesMoved = 10 - Position
                InJail = True
            Else
                If InJail = False Then
                    If IsHuman() Then
                        Console.WriteLine("You have rolled a double. You can roll the dice again.")
                    Else
                        Console.BackgroundColor = PlayerColour
                        Console.Write(username)
                        Console.BackgroundColor = ConsoleColor.Black
                        Console.WriteLine("has rolled a double so can roll the dice again.")
                    End If
                End If
            End If
        Else
            doubles = 0
        End If

        Return (SpacesMoved, dice1 = dice2)

    End Function

    Function Move(SpacesMoved As Integer, Board As List(Of BoardPosition)) As Integer

        Position += SpacesMoved
        If Position > 39 Then
            Position -= 40
            If IsHuman() Then
                Console.WriteLine("You have passed go - collect £200.")
            Else
                Console.BackgroundColor = PlayerColour
                Console.Write(username)
                Console.BackgroundColor = ConsoleColor.Black
                Console.WriteLine(" has passed go so collects £200.")
            End If
            ChangeMoney(200, True, Nothing)
        End If

        If Position < 0 Then
            Position += 40
        End If
        Dim Square As BoardPosition = Board(Position)

        If IsHuman() Then
            Console.Write("You have moved " & SpacesMoved & " spaces and landed on ")
        Else
            Console.BackgroundColor = PlayerColour
            Console.Write(username)
            Console.BackgroundColor = ConsoleColor.Black
            Console.Write(" has moved " & SpacesMoved & " spaces and landed on ")
        End If

        If Square.GetType() = GetType(BoardProperty) Then
            Dim Prop As BoardProperty = Square
            Dim Colour As String = Prop.Colour
            ColourCheck(Colour)
        End If

        Console.WriteLine(Square.Name)

        Console.ResetColor()

        Console.WriteLine()

        Select Case Square.GetType()
            Case GetType(BoardOther)
            Case GetType(BoardChest)
                CommunityChest()
            Case GetType(BoardTax)
                Tax(Square)
            Case GetType(BoardChance)
                Chance()
            Case GetType(BoardGoToJail)
                Position = 10
                Jail()
                Console.WriteLine()
            Case GetType(BoardProperty)
                PropertyCheck(Square)
            Case GetType(BoardStation)
                Station(Square)
            Case GetType(BoardUtility)
                Utility(Square, SpacesMoved)
        End Select

        Return Position

    End Function



    Public Sub CommunityChest()

        Dim SpacesMoved As Integer = 0

        Console.WriteLine("Press enter to pick a community chest card.")
        If IsHuman() Then
            Console.ReadLine()
        End If

        CommunityChestCard = CommunityChestCards.Dequeue()

        Select Case CommunityChestCard
            Case 0
                Console.WriteLine("Pay your insurance premium £50.")
                ChangeMoney(-50, True, Nothing)
            Case 1
                Console.WriteLine("From sale of stock you get £50.")
                ChangeMoney(50, True, Nothing)
            Case 2
                Dim choice As String = ""
                Do Until choice = "1" Or choice = "2"
                    Console.WriteLine("Press 1 to pay a £10 fine, or press 2 to take a 'chance'")
                    If IsHuman() Then
                        choice = Console.ReadLine()
                    Else
                        Randomize()
                        choice = Int((2 * Rnd()) + 1).ToString
                        Console.WriteLine(choice)
                    End If
                    If choice = "1" Then
                        ChangeMoney(-10, True, Nothing)
                    ElseIf choice = "2" Then
                        Chance()
                    Else
                        Console.WriteLine("Input invalid.")
                    End If
                Loop
            Case 3
                Console.WriteLine("Bank error in your favour.")
                Console.WriteLine("Collect £200.")
                ChangeMoney(200, True, Nothing)
            Case 4
                Console.WriteLine("It is you birthday.")
                Console.WriteLine("Collect £10 from each player.")
                Dim total As Integer = 0
                For Each otherplayer As Player In ListOfPlayers
                    If otherplayer.username <> username Then
                        If otherplayer.ChangeMoney(-10, False, Me) = True Then
                            total += 10
                        End If
                    End If
                Next
                ChangeMoney(total, True, Nothing)
            Case 5
                Console.WriteLine("Get out of jail free.")
                Console.WriteLine("This can be kept until needed or sold")
                DoesPlayerHaveCommunityChestGetOutOfJailCard = True
            Case 6
                Console.WriteLine("Go to jail.")
                Console.WriteLine("Go directly to jail - do not pass go or collect £200.")
                SpacesMoved = 10 - Position
                Move(SpacesMoved, Board)
                Jail()
            Case 7
                Console.WriteLine("You inherit £100.")
                ChangeMoney(100, True, Nothing)
            Case 8
                Console.WriteLine("You have won second prize in a beauty contest.")
                Console.WriteLine("Collect £10.")
                ChangeMoney(10, True, Nothing)
            Case 9
                Console.WriteLine("Pay hospital £100.")
                ChangeMoney(-100, True, Nothing)
            Case 10
                Console.WriteLine("Income tax refund.")
                Console.WriteLine("Collect £20.")
                ChangeMoney(20, True, Nothing)
            Case 11
                Console.WriteLine("Receive interest on 7% preference shares.")
                Console.WriteLine("You receive £25.")
                ChangeMoney(25, True, Nothing)
            Case 12
                Console.WriteLine("Annuity matures.")
                Console.WriteLine("Collect £100.")
                ChangeMoney(100, True, Nothing)
            Case 13
                Console.WriteLine("Go back to Old Kent Road")
                SpacesMoved = 1 - Position
                Move(SpacesMoved, Board)
            Case 14
                Console.WriteLine("Doctor's fee.")
                Console.WriteLine("Pay £50.")
                ChangeMoney(-50, True, Nothing)
            Case 15
                Console.WriteLine("Advance to Go.")
                SpacesMoved = 40 - Position
                Move(SpacesMoved, Board)
        End Select

        If CommunityChestCard <> 5 Then
            CommunityChestCards.Enqueue(CommunityChestCard)
        End If

        Console.WriteLine()

    End Sub

    Sub Chance()

        Dim SpacesMoved As Integer = 0
        Dim HouseCost, HotelCost As Integer

        Console.WriteLine("Press enter to pick a chance card.")
        If IsHuman() Then
            Console.ReadLine()
        End If

        ChanceCard = ChanceCards.Dequeue()

        Select Case ChanceCard
            Case 0
                Console.WriteLine("Get out of jail free.")
                Console.WriteLine("This can be kept until needed or sold")
                DoesPlayerHaveChanceGetOutOfJailCard = True
            Case 1
                Console.WriteLine("Go back 3 spaces.")
                SpacesMoved = -3
                Move(SpacesMoved, Board)
            Case 2
                Console.WriteLine("Advance to Trafalgar Square")
                If Position > 24 Then
                    SpacesMoved = (40 - Position) + 24
                Else
                    SpacesMoved = 24 - Position
                End If
                Move(SpacesMoved, Board)
            Case 3
                Console.WriteLine("Drunk in charge.")
                Console.WriteLine("Fine £20")
                ChangeMoney(-20, True, Nothing)
            Case 4
                Console.WriteLine("Advance to Go.")
                SpacesMoved = 40 - Position
                Move(SpacesMoved, Board)
            Case 5
                Console.WriteLine("Bank pays you dividend of £50.")
                ChangeMoney(50, True, Nothing)
            Case 6
                Console.WriteLine("Take a trip to Marylebone Station.")
                If Position > 15 Then
                    SpacesMoved = (40 - Position) + 15
                Else
                    SpacesMoved = 15 - Position
                End If
                Move(SpacesMoved, Board)
            Case 7
                Console.WriteLine("You are assessed for street repairs.")
                Console.WriteLine("£40 per house.")
                Console.WriteLine("£115 per hotel.")
                HouseCost = 40
                HotelCost = 115
                BuildingRepairs(HouseCost, HotelCost)
            Case 8
                Console.WriteLine("Advance to Pall Mall.")
                If Position > 11 Then
                    SpacesMoved = (40 - Position) + 11
                Else
                    SpacesMoved = 11 - Position
                End If
                Move(SpacesMoved, Board)
            Case 9
                Console.WriteLine("Pay school fees of £150.")
                ChangeMoney(-150, True, Nothing)
            Case 10
                Console.WriteLine("Advance to Mayfair.")
                If Position > 39 Then
                    SpacesMoved = (40 - Position) + 39
                Else
                    SpacesMoved = 39 - Position
                End If
                Move(SpacesMoved, Board)
            Case 11
                Console.WriteLine("You have won a crossword competition.")
                Console.WriteLine("Collect £100.")
                ChangeMoney(100, True, Nothing)
            Case 12
                Console.WriteLine("Go to jail.")
                Console.WriteLine("Go directly to jail - do not pass go or collect £200.")
                SpacesMoved = 10 - Position
                Move(SpacesMoved, Board)
                Jail()
            Case 13
                Console.WriteLine("Make general repairs on all of your buildings.")
                Console.WriteLine("£25 per house.")
                Console.WriteLine("£100 per hotel.")
                HouseCost = 25
                HotelCost = 100
                BuildingRepairs(HouseCost, HotelCost)
            Case 14
                Console.WriteLine("Speeding fine £15.")
                ChangeMoney(-15, True, Nothing)
            Case 15
                Console.WriteLine("Your building loan matures.")
                Console.WriteLine("Receive £150")
                ChangeMoney(150, True, Nothing)
        End Select

        If ChanceCard <> 0 Then
            ChanceCards.Enqueue(ChanceCard)
        End If

        Console.WriteLine()

    End Sub



    Sub Tax(Square)

        Console.WriteLine("You must pay £" & Square.PriceToPay & ".")
        ChangeMoney(-Square.PriceToPay, True, Nothing)
        Console.WriteLine()

    End Sub



    Sub PropertyCheck(Square As BoardProperty)

        Dim PriceToPay As Integer

        If Square.Owner Is Me Then
            If IsHuman() Then
                Console.WriteLine("You already own this property.")
                Console.WriteLine()
            End If
        ElseIf Square.Owner IsNot Nothing Then
            If Square.IsMortgaged Then
                If IsHuman() Then
                    Console.Write("This property is currently mortaged so you do not owe ")
                    Console.BackgroundColor = Square.Owner.PlayerColour
                    Console.Write(Square.Owner.username)
                    Console.BackgroundColor = ConsoleColor.Black
                    Console.WriteLine(" any rent.")
                Else
                    Console.WriteLine("This property is currently mortaged so ")
                    Console.BackgroundColor = PlayerColour
                    Console.Write(username)
                    Console.BackgroundColor = ConsoleColor.Black
                    Console.Write(" does not owe ")
                    Console.BackgroundColor = Square.Owner.PlayerColour
                    Console.Write(Square.Owner.username)
                    Console.BackgroundColor = ConsoleColor.Black
                    Console.WriteLine(" any rent.")
                End If
                Console.WriteLine()
            Else
                PriceToPay = Square.GetRent()
                If IsHuman() Then
                    Console.Write("You owe ")
                Else
                    Console.BackgroundColor = PlayerColour
                    Console.Write(username)
                    Console.BackgroundColor = ConsoleColor.Black
                    Console.Write(" owes ")
                End If
                Console.BackgroundColor = Square.Owner.PlayerColour
                Console.Write(Square.Owner.username)
                Console.BackgroundColor = ConsoleColor.Black
                Console.WriteLine(" £" & PriceToPay)
                If ChangeMoney(-PriceToPay, True, Square.Owner) = True Then
                    Square.Owner.ChangeMoney(PriceToPay, False, Nothing)
                Else
                    BankruptPropertyUpdate(Square.Owner)
                End If
                Console.WriteLine()
            End If
        ElseIf Square.PriceToBuy > money Then
            If IsHuman() Then
                Console.WriteLine("You do not have enough money to buy this property.")
                Console.WriteLine()
            End If
        Else
            If DecisionBuyProperty(Square, Square.PriceToBuy) Then
                Dim sold As Boolean = False
                PropertyOwnerUpdate(Square, sold, Nothing)
                ChangeMoney(-Square.PriceToBuy, True, Nothing)
                Console.WriteLine()
            End If
        End If

    End Sub

    Sub PropertyOwnerUpdate(Square, sold, ChosenPlayer)

        If sold = False Then
            Square.Owner = Me
            If IsHuman() Then
                Console.Write("You now own ")
            Else
                Console.BackgroundColor = PlayerColour
                Console.Write(username)
                Console.BackgroundColor = ConsoleColor.Black
                Console.Write(" now owns ")
            End If
            If Square.GetType() = GetType(BoardProperty) Then
                Dim Prop As BoardProperty = Square
                Dim Colour As String = Prop.Colour
                ColourCheck(Colour)
            End If
            Console.WriteLine(Square.Name)
            Console.ResetColor()
        Else
            Square.Owner = ChosenPlayer
        End If

        If Square.GetType = GetType(BoardProperty) Then
            If NumberOfPropertiesOwned(Square.Colour) = Square.NumberOfProperties Then
                If sold = False Then
                    If IsHuman() Then
                        Console.Write("You now own all ")
                    Else
                        Console.BackgroundColor = PlayerColour
                        Console.Write(username)
                        Console.BackgroundColor = ConsoleColor.Black
                        Console.Write(" now owns all ")
                    End If
                Else
                    Console.BackgroundColor = ChosenPlayer.PlayerColour
                    Console.Write(ChosenPlayer.username)
                    Console.BackgroundColor = ConsoleColor.Black
                    Console.Write(" now owns all ")
                End If
                Dim Prop As BoardProperty = Square
                Dim Colour As String = Prop.Colour
                ColourCheck(Colour)
                Console.Write(Square.Colour)
                Console.ResetColor()
                Console.WriteLine(" properties, and can now build on these properties.")
            End If
        End If

    End Sub

    Function DoesOtherPlayerOwnAllProperties(Colour, Owner) As Boolean

        For Each Square As BoardProperty In ListOfBoardProperties
            If Square.Colour = Colour And Square.Owner IsNot Owner Then
                Return False
            End If
        Next

        Return True

    End Function

    Function NumberOfPropertiesOwned(Colour) As Integer

        Dim Number As Integer = 0

        For Each Square As BoardProperty In ListOfBoardProperties
            If Square.Owner IsNot Nothing Then
                If Square.Colour = Colour And Square.Owner Is Me And Square.IsMortgaged = False Then
                    Number += 1
                End If
            End If
        Next

        Return Number

    End Function

    Sub BankruptPropertyUpdate(OwedPlayer As Player)

        Dim TotalMoney As Integer = 0

        IsBankrupt = True

        Console.WriteLine()
        For Each prop As SellablePosition In ListOfSellablePositions
            If prop.Owner Is Me Then
                If prop.GetType() = GetType(BoardProperty) Then
                    Dim boardProperty As BoardProperty = prop
                    TotalMoney += boardProperty.NumberOfHouses * boardProperty.BuildingCost * 0.5
                    TotalMoney += boardProperty.NumberOfHotels * boardProperty.BuildingCost * 5 * 0.5
                    boardProperty.NumberOfHotels = 0
                    boardProperty.NumberOfHouses = 0
                End If
                If Not OwedPlayer Is Nothing Then
                    prop.Owner = OwedPlayer
                    Console.BackgroundColor = OwedPlayer.PlayerColour
                    Console.Write(OwedPlayer.username)
                    Console.BackgroundColor = ConsoleColor.Black
                    Console.Write(" now owns ")
                    If prop.GetType() = GetType(BoardProperty) Then
                        Dim boardproperty As BoardProperty = prop
                        Dim Colour As String = boardproperty.Colour
                        ColourCheck(Colour)
                    End If
                    Console.WriteLine(prop.Name)
                    Console.ResetColor()
                End If
            End If
        Next

        If Not OwedPlayer Is Nothing Then
            Console.WriteLine()
            OwedPlayer.money += TotalMoney
            Console.Write("Buildings on these properties have been sold to the bank, so ")
            Console.BackgroundColor = OwedPlayer.PlayerColour
            Console.Write(OwedPlayer.username)
            Console.BackgroundColor = ConsoleColor.Black
            Console.WriteLine(" has earnt £" & TotalMoney)

            Dim number As Integer = 0

            If DoesPlayerHaveChanceGetOutOfJailCard = True Then
                DoesPlayerHaveChanceGetOutOfJailCard = False
                OwedPlayer.DoesPlayerHaveChanceGetOutOfJailCard = True
                number += 1
            End If
            If DoesPlayerHaveCommunityChestGetOutOfJailCard = True Then
                DoesPlayerHaveCommunityChestGetOutOfJailCard = False
                OwedPlayer.DoesPlayerHaveCommunityChestGetOutOfJailCard = True
                number += 1
            End If

            If number = 1 Then
                Console.BackgroundColor = OwedPlayer.PlayerColour
                Console.Write(OwedPlayer.username)
                Console.BackgroundColor = ConsoleColor.Black
                Console.Write(" has also received ")
                Console.BackgroundColor = PlayerColour
                Console.Write(username)
                Console.BackgroundColor = ConsoleColor.Black
                Console.WriteLine("'s get out of jail free card")
            ElseIf number = 2 Then
                Console.BackgroundColor = OwedPlayer.PlayerColour
                Console.Write(OwedPlayer.username)
                Console.BackgroundColor = ConsoleColor.Black
                Console.Write(" has also received both of ")
                Console.BackgroundColor = PlayerColour
                Console.Write(username)
                Console.BackgroundColor = ConsoleColor.Black
                Console.WriteLine("'s get out of jail free cards")
            End If
        Else
            Console.Write("All of ")
            Console.BackgroundColor = PlayerColour
            Console.Write(username)
            Console.BackgroundColor = ConsoleColor.Black
            Console.WriteLine("'s properties and building have been sold to the bank.")
            If DoesPlayerHaveChanceGetOutOfJailCard Then
                ChanceCards.Enqueue(0)
                DoesPlayerHaveChanceGetOutOfJailCard = False
            Else
                CommunityChestCards.Enqueue(5)
                DoesPlayerHaveCommunityChestGetOutOfJailCard = False
            End If
        End If

    End Sub


    Sub Station(Square As BoardStation)

        Dim PriceToPay As Integer

        If Square.Owner Is Me Then
            If IsHuman() Then
                Console.WriteLine("You already own this station.")
                Console.WriteLine()
            End If
        ElseIf Square.Owner IsNot Nothing Then
            If Square.IsMortgaged Then
                If IsHuman() Then
                    Console.Write("This station is currently mortaged so you do not owe ")
                    Console.BackgroundColor = Square.Owner.PlayerColour
                    Console.Write(Square.Owner.username)
                    Console.BackgroundColor = ConsoleColor.Black
                    Console.WriteLine(" any rent.")
                Else
                    Console.Write("This station is currently mortaged so ")
                    Console.BackgroundColor = PlayerColour
                    Console.Write(username)
                    Console.BackgroundColor = ConsoleColor.Black
                    Console.Write("does not owe ")
                    Console.BackgroundColor = Square.Owner.PlayerColour
                    Console.Write(Square.Owner.username)
                    Console.BackgroundColor = ConsoleColor.Black
                    Console.WriteLine(" any rent.")
                End If
                Console.WriteLine()
            Else
                PriceToPay = Square.GetRent()
                If IsHuman() Then
                    Console.Write("You owe ")
                Else
                    Console.BackgroundColor = PlayerColour
                    Console.Write(username)
                    Console.BackgroundColor = ConsoleColor.Black
                    Console.Write(" owes ")
                End If
                Console.BackgroundColor = Square.Owner.PlayerColour
                Console.Write(Square.Owner.username)
                Console.BackgroundColor = ConsoleColor.Black
                Console.WriteLine(" £" & PriceToPay)
                If ChangeMoney(-PriceToPay, True, Square.Owner) = True Then
                    Square.Owner.ChangeMoney(PriceToPay, False, Nothing)
                Else
                    BankruptPropertyUpdate(Square.Owner)
                End If
                Console.WriteLine()
            End If
        ElseIf Square.PriceToBuy > money Then
            If IsHuman() Then
                Console.WriteLine("You do not have enough money to buy this station.")
                Console.WriteLine()
            End If
        Else
            If DecisionBuyStation(Square) Then
                Dim sold As Boolean = False
                PropertyOwnerUpdate(Square, sold, Nothing)
                ChangeMoney(-Square.PriceToBuy, True, Nothing)
                Console.WriteLine()
            End If
        End If

    End Sub

    Function NumberOfStationsOwned() As Integer

        Dim Number As Integer = 0

        For Each Square As BoardStation In ListOfBoardStations
            If Square.Owner IsNot Nothing Then
                If Square.Owner Is Me Then
                    Number += 1
                End If
            End If
        Next

        Return Number

    End Function

    Sub StationOwnerUpdate(Square, sold, ChosenPlayer)

        If sold = False Then
            Square.Owner = Me
            If IsHuman() Then
                Console.WriteLine("You now own " & Square.Name)
            Else
                Console.BackgroundColor = PlayerColour
                Console.Write(username)
                Console.BackgroundColor = ConsoleColor.Black
                Console.WriteLine(" now owns " & Square.Name)
            End If
        Else
            Square.Owner = ChosenPlayer
        End If

        If NumberOfStationsOwned() = 4 Then
            If sold = False Then
                If IsHuman() Then
                    Console.WriteLine("You now own all stations.")
                Else
                    Console.BackgroundColor = PlayerColour
                    Console.Write(username)
                    Console.BackgroundColor = ConsoleColor.Black
                    Console.WriteLine(" now owns all stations.")
                End If
            Else
                Console.BackgroundColor = ChosenPlayer.PlayerColour
                Console.Write(ChosenPlayer.username)
                Console.BackgroundColor = ConsoleColor.Black
                Console.WriteLine(" now owns all stations.")
            End If
        End If

    End Sub



    Sub Utility(Square As BoardUtility, SpacesMoved As Integer)

        Dim PriceToPay As Integer

        If Square.Owner Is Me Then
            If IsHuman() Then
                Console.WriteLine("You already own this utility.")
                Console.WriteLine()
            End If
        ElseIf Square.Owner IsNot Nothing Then
            If Square.IsMortgaged = False Then
                PriceToPay = Square.GetRent() * SpacesMoved
                If IsHuman() Then
                    Console.Write("You owe ")
                Else
                    Console.BackgroundColor = PlayerColour
                    Console.Write(username)
                    Console.BackgroundColor = ConsoleColor.Black
                    Console.Write(" now owes ")
                End If
                Console.BackgroundColor = Square.Owner.PlayerColour
                Console.Write(Square.Owner.username)
                Console.BackgroundColor = ConsoleColor.Black
                Console.WriteLine(" £" & PriceToPay)
                If ChangeMoney(-PriceToPay, True, Square.Owner) = True Then
                    Square.Owner.ChangeMoney(PriceToPay, False, Nothing)
                Else
                    BankruptPropertyUpdate(Square.Owner)
                End If
                Console.WriteLine()
            Else
                If IsHuman() Then
                    Console.Write("This utility is currently mortaged so you do not owe ")
                    Console.BackgroundColor = Square.Owner.PlayerColour
                    Console.Write(Square.Owner.username)
                    Console.BackgroundColor = ConsoleColor.Black
                    Console.WriteLine(" any rent.")
                Else
                    Console.Write("This utility is currently mortaged so ")
                    Console.BackgroundColor = PlayerColour
                    Console.Write(username)
                    Console.BackgroundColor = ConsoleColor.Black
                    Console.Write(" does not owe ")
                    Console.BackgroundColor = Square.Owner.PlayerColour
                    Console.Write(Square.Owner.username)
                    Console.BackgroundColor = ConsoleColor.Black
                    Console.WriteLine(" any rent.")
                End If
                Console.WriteLine()
            End If
        Else
            If Square.PriceToBuy > money Then
                If IsHuman() Then
                    Console.WriteLine("You do not have enough money to buy this utility.")
                    Console.WriteLine()
                End If
            Else
                If DecisionBuyUtility(Square) Then
                    Dim sold As Boolean = False
                    PropertyOwnerUpdate(Square, sold, Nothing)
                    ChangeMoney(-Square.PriceToBuy, True, Nothing)
                    Console.WriteLine()
                End If
            End If
        End If

    End Sub

    Function NumberOfUtilitiesOwned() As Integer

        Dim Number As Integer = 0

        For Each Square As BoardUtility In ListOfBoardUtilities
            If Square.Owner IsNot Nothing Then
                If Square.Owner Is Me Then
                    Number += 1
                End If
            End If
        Next

        Return Number

    End Function

    Sub UtilityOwnerUpdate(Square, sold, ChosenPlayer)

        If sold = False Then
            Square.Owner = Me
            If IsHuman() Then
                Console.WriteLine("You now own " & Square.Name)
            Else
                Console.BackgroundColor = PlayerColour
                Console.Write(username)
                Console.BackgroundColor = ConsoleColor.Black
                Console.WriteLine(" now owns " & Square.name)
            End If
        Else
            Square.Owner = ChosenPlayer
        End If

        If NumberOfUtilitiesOwned() = 2 Then
            If sold = False Then
                If IsHuman() Then
                    Console.WriteLine("You now own all utilities.")
                Else
                    Console.BackgroundColor = PlayerColour
                    Console.Write(username)
                    Console.BackgroundColor = ConsoleColor.Black
                    Console.WriteLine(" now owns all utilities.")
                End If
            Else
                Console.BackgroundColor = ChosenPlayer.PlayerColour
                Console.Write(ChosenPlayer.username)
                Console.BackgroundColor = ConsoleColor.Black
                Console.WriteLine(" now owns all utilities.")
            End If
        End If

    End Sub



    Sub Jail()

        InJail = True
        JailTurnsRemaining = 3
        If IsHuman() Then
            Console.WriteLine("You are now in Jail.")
        Else
            Console.BackgroundColor = PlayerColour
            Console.Write(username)
            Console.BackgroundColor = ConsoleColor.Black
            Console.WriteLine(" is now in Jail.")
        End If
        Console.WriteLine()

    End Sub

    Sub GetOutOfJail()

        Dim NumberOfGetOutOfJailCards As Integer = 0

        If DoesPlayerHaveChanceGetOutOfJailCard Then
            NumberOfGetOutOfJailCards += 1
        End If

        If DoesPlayerHaveCommunityChestGetOutOfJailCard Then
            NumberOfGetOutOfJailCards += 1
        End If

        If NumberOfGetOutOfJailCards > 0 Then
            If DecisionUseGetOutOfJailFreeCard(NumberOfGetOutOfJailCards) Then
                If IsHuman() Then
                    Console.WriteLine("You are no longer in jail.")
                Else
                    Console.BackgroundColor = PlayerColour
                    Console.Write(username)
                    Console.BackgroundColor = ConsoleColor.Black
                    Console.WriteLine(" used a get out of jail free card and is no longer in jail.")
                End If
                If DoesPlayerHaveChanceGetOutOfJailCard Then
                    ChanceCards.Enqueue(0)
                    DoesPlayerHaveChanceGetOutOfJailCard = False
                Else
                    CommunityChestCards.Enqueue(5)
                    DoesPlayerHaveCommunityChestGetOutOfJailCard = False
                End If
                If NumberOfGetOutOfJailCards = 2 Then
                    If IsHuman() Then
                        Console.WriteLine("You now have 1 get out of jail free card left.")
                    Else
                        Console.BackgroundColor = PlayerColour
                        Console.Write(username)
                        Console.BackgroundColor = ConsoleColor.Black
                        Console.WriteLine(" now has 1 get out of jail free card left.")
                    End If
                Else
                    If IsHuman() Then
                        Console.WriteLine("You do not have any get out of jail free cards left.")
                    Else
                        Console.BackgroundColor = PlayerColour
                        Console.Write(username)
                        Console.BackgroundColor = ConsoleColor.Black
                        Console.WriteLine(" does not have any get out of jail free cards left.")
                    End If
                End If
            End If
        Else
            If money >= 50 Then
                If DecisionPayToGetOutOfJail() Then
                    ChangeMoney(-50, True, Nothing)
                    InJail = False
                    If IsHuman() Then
                        Console.WriteLine("You are no longer in jail.")
                    Else
                        Console.BackgroundColor = PlayerColour
                        Console.Write(username)
                        Console.BackgroundColor = ConsoleColor.Black
                        Console.WriteLine(" has paid £50 to get out of jail.")
                    End If
                End If
            End If
        End If

    End Sub

    Sub GetHouseHotelBuildLists(HouseBuildList As List(Of BoardProperty), HotelBuildList As List(Of BoardProperty), HouseBuildColours As HashSet(Of String), HotelBuildColours As HashSet(Of String))

        Dim PropertiesThatCanBeBuiltOn As New List(Of BoardProperty)

        For Each PropertyToBuild As BoardProperty In ListOfBoardProperties
            If NumberOfPropertiesOwned(PropertyToBuild.Colour) = PropertyToBuild.NumberOfProperties Then
                PropertiesThatCanBeBuiltOn.Add(PropertyToBuild)
            End If
        Next

        'get list of properties that houses and hotels can be built on
        For Each Prop As BoardProperty In PropertiesThatCanBeBuiltOn
            Dim CanHouseBeBuilt As Boolean = True
            Dim CanHotelBeBuilt As Boolean = True
            For Each OtherProp As BoardProperty In PropertiesThatCanBeBuiltOn
                If Prop.Colour = OtherProp.Colour Then
                    If Prop.NumberOfHouses > OtherProp.NumberOfHouses Or Prop.NumberOfHouses = 4 Or Prop.NumberOfHotels > 0 Then
                        CanHouseBeBuilt = False
                    End If
                    If (Prop.NumberOfHouses < 4 Or Prop.NumberOfHotels = 1) Or (OtherProp.NumberOfHouses < 4 And OtherProp.NumberOfHotels = 0) Then
                        CanHotelBeBuilt = False
                    End If
                End If
            Next
            If CanHouseBeBuilt = True Then
                HouseBuildList.Add(Prop)
                HouseBuildColours.Add(Prop.Colour)
            ElseIf CanHotelBeBuilt = True Then
                HotelBuildList.Add(Prop)
                HotelBuildColours.Add(Prop.Colour)
            End If
        Next

    End Sub


    Sub HouseBuy(ChosenProperty As BoardProperty)


        ChosenProperty.NumberOfHouses += 1
        NumberOfHousesAvailable -= 1
        ChangeMoney(-ChosenProperty.BuildingCost, True, Nothing)
        Console.WriteLine()
        If IsHuman() Then
            Console.Write("You now have ")
        Else
            Console.BackgroundColor = PlayerColour
            Console.Write(username)
            Console.BackgroundColor = ConsoleColor.Black
            Console.Write(" has bought a house and now has ")
        End If
        Console.Write(ChosenProperty.NumberOfHouses & " houses on ")
        ColourCheck(ChosenProperty.Colour)
        Console.WriteLine(ChosenProperty.Name)
        Console.ResetColor()

        If ChosenProperty.NumberOfHouses = 4 Then
            Dim Number As Integer = 0
            For Each Square As BoardProperty In ListOfBoardProperties
                If Square.Owner IsNot Nothing Then
                    If Square.Colour = ChosenProperty.Colour And Square.Owner Is Me And Square.NumberOfHouses = 4 Then
                        Number += 1
                    End If
                End If
            Next
            If Number = ChosenProperty.NumberOfProperties And ChosenProperty.NumberOfHotels = 0 Then
                If IsHuman() Then
                    Console.Write("You now have")
                Else
                    Console.BackgroundColor = PlayerColour
                    Console.Write(username)
                    Console.BackgroundColor = ConsoleColor.Black
                    Console.Write(" now has")
                End If
                Console.Write(" a full set of houses on every ")
                ColourCheck(ChosenProperty.Colour)
                Console.Write(ChosenProperty.Colour)
                Console.ResetColor()
                Console.Write(" property. ")
                If IsHuman() Then
                    Console.WriteLine("You can now build hotels on these properties.")
                Else
                    Console.BackgroundColor = PlayerColour
                    Console.Write(username)
                    Console.BackgroundColor = ConsoleColor.Black
                    Console.WriteLine(" can now build hotels on these properties.")
                End If
            End If
        End If

    End Sub

    Sub HotelBuy(ChosenProperty As BoardProperty)

        ChosenProperty.NumberOfHouses = 0
        ChosenProperty.NumberOfHotels = 1
        NumberOfHousesAvailable += 4
        NumberOfHotelsAvailable -= 1
        ChangeMoney(-ChosenProperty.BuildingCost, True, Nothing)
        Console.WriteLine()
        If IsHuman() Then
            Console.Write("You now have a hotel on ")
        Else
            Console.BackgroundColor = PlayerColour
            Console.Write(username)
            Console.BackgroundColor = ConsoleColor.Black
            Console.Write(" has bought a hotel on ")
        End If
        ColourCheck(ChosenProperty.Colour)
        Console.WriteLine(ChosenProperty.Name)
        Console.ResetColor()
        Console.WriteLine()

    End Sub

    Protected Sub SellHotel(BuildingProperty As BoardProperty)
        BuildingProperty.NumberOfHouses = 4
        BuildingProperty.NumberOfHotels = 0
        NumberOfHousesAvailable -= 4
        NumberOfHotelsAvailable += 1
        ChangeMoney(BuildingProperty.BuildingCost, True, Nothing)
        If IsHuman() Then
            Console.Write("You now have no hotels and 4 houses on ")
        Else
            Console.BackgroundColor = PlayerColour
            Console.Write(username)
            Console.BackgroundColor = ConsoleColor.Black
            Console.Write(" has sold their hotel and now has no hotels and 4 houses on ")
        End If
        ColourCheck(BuildingProperty.Colour)
        Console.WriteLine(BuildingProperty.Name)
        Console.ResetColor()
    End Sub

    Protected Sub SellHouse(BuildingProperty As BoardProperty)
        BuildingProperty.NumberOfHouses -= 1
        NumberOfHousesAvailable += 1
        ChangeMoney(BuildingProperty.BuildingCost, True, Nothing)
        If IsHuman() Then
            Console.Write("You now have " & BuildingProperty.NumberOfHouses & " houses on ")
        Else
            Console.BackgroundColor = PlayerColour
            Console.Write(username)
            Console.BackgroundColor = ConsoleColor.Black
            Console.Write(" has sold a house and now has " & BuildingProperty.NumberOfHouses & " houses on ")
        End If
        ColourCheck(BuildingProperty.Colour)
        Console.WriteLine(BuildingProperty.Name)
        Console.ResetColor()
    End Sub

    'NEED TO CHANGE FOR COMPUTER OPPONENT
    Sub SellProperties(ListOfPlayers As List(Of Player))

        Dim AreThereAnyPropertiesToSell As Boolean = False
        Dim TempListOfProperties As New List(Of BoardProperty)
        Dim ListOfProperties As New List(Of SellablePosition)
        Dim ColoursWithBuildings As New List(Of String)

        Console.WriteLine()

        For Each prop As BoardProperty In ListOfBoardProperties
            If prop.Owner IsNot Nothing Then
                If prop.Owner Is Me Then
                    TempListOfProperties.Add(prop)
                End If
            End If
        Next

        For Each prop As BoardProperty In TempListOfProperties
            If prop.NumberOfHotels > 0 Or prop.NumberOfHouses > 0 Then
                ColoursWithBuildings.Add(prop.Colour)
            End If
        Next

        For Each prop As BoardProperty In TempListOfProperties
            If Not ColoursWithBuildings.Contains(prop.Colour) Then
                ListOfProperties.Add(prop)
            End If
        Next

        For Each Station As SellablePosition In ListOfBoardStations
            If Station.Owner IsNot Nothing Then
                If Station.Owner Is Me Then
                    ListOfProperties.Add(Station)
                End If
            End If
        Next

        For Each Utility As SellablePosition In ListOfBoardUtilities
            If Utility.Owner IsNot Nothing Then
                If Utility.Owner Is Me Then
                    ListOfProperties.Add(Utility)
                End If
            End If
        Next

        If ListOfProperties.Count > 0 Then
            AreThereAnyPropertiesToSell = True
        End If

        Console.WriteLine()
        If AreThereAnyPropertiesToSell = True Then
            Console.WriteLine("You can currently sell these properties:")
            For Each Properties As SellablePosition In ListOfProperties
                If Properties.GetType() = GetType(BoardProperty) Then
                    Dim Prop As BoardProperty = Properties
                    Dim Colour As String = Prop.Colour
                    ColourCheck(Colour)
                End If
                Console.WriteLine(Properties.Name)
                Console.ResetColor()
            Next
        Else
            Console.WriteLine("You do not have any properties to sell.")
        End If
        Console.WriteLine("Any other owned properties cannot be sold until buildings on all colours of that property are sold.")

        Dim sold As Boolean = False
        Dim SellPrice As Integer
        Dim PlayersWantingToBuy As New List(Of Player)
        Dim ChosenPlayer As Player
        Dim PropertyName As String
        Dim valid As Boolean = False
        Dim ExitSub As Boolean = False

        Do Until valid = True Or ExitSub = True Or ListOfProperties.Count = 0
            Console.WriteLine()
            Console.WriteLine("Enter the name of the property you want to sell, or enter 5 to exit.")
            PropertyName = Console.ReadLine()
            For Each Properties As SellablePosition In ListOfProperties
                If PropertyName.ToUpper = Properties.Name.ToUpper Then
                    valid = True
                    PropertyName = Properties.Name
                    Do Until sold = True Or ExitSub = True
                        Console.WriteLine("How much do you want to sell this property for?")
                        Console.Write("£")
                        If Integer.TryParse(Console.ReadLine(), SellPrice) Then
                            PlayersWantingToBuy.Clear()
                            For Each otherplayer As Player In ListOfPlayers
                                If otherplayer.username <> username Then
                                    If otherplayer.DecisionBuyProperty(Properties, SellPrice) Then
                                        If otherplayer.money >= SellPrice Then
                                            PlayersWantingToBuy.Add(otherplayer)
                                        Else
                                            Console.WriteLine("You do not have enough money to buy this property.")
                                        End If
                                    End If
                                End If
                            Next
                            If PlayersWantingToBuy.Count = 0 Then
                                Console.WriteLine()
                                Console.Write("No players wanted to buy ")
                                If Properties.GetType() = GetType(BoardProperty) Then
                                    Dim Prop As BoardProperty = Properties
                                    Dim Colour As String = Prop.Colour
                                    ColourCheck(Colour)
                                End If
                                Console.Write(PropertyName)
                                Console.ResetColor()
                                Console.WriteLine(" for £" & SellPrice)
                                Dim choice2 As String = ""
                                Do Until choice2 = "Y" Or choice2 = "N"
                                    Console.Write("Do you want to try selling ")
                                    If Properties.GetType() = GetType(BoardProperty) Then
                                        Dim Prop As BoardProperty = Properties
                                        Dim Colour As String = Prop.Colour
                                        ColourCheck(Colour)
                                    End If
                                    Console.Write(PropertyName)
                                    Console.ResetColor()
                                    Console.WriteLine(" for a different price?")
                                    Console.WriteLine("Enter Y for yes and N for no.")
                                    choice2 = Console.ReadLine().ToUpper
                                    If choice2 = "N" Then
                                        ExitSub = True
                                    ElseIf choice2 <> "Y" Then
                                        Console.WriteLine("Input invalid.")
                                    End If
                                Loop
                            ElseIf PlayersWantingToBuy.Count = 1 Then
                                Dim choice As String = ""
                                ChosenPlayer = PlayersWantingToBuy(0)
                                Do Until choice = "Y" Or choice = "N"
                                    Console.WriteLine()
                                    Console.Write("Do you want to sell ")
                                    If Properties.GetType() = GetType(BoardProperty) Then
                                        Dim Prop As BoardProperty = Properties
                                        Dim Colour As String = Prop.Colour
                                        ColourCheck(Colour)
                                    End If
                                    Console.Write(PropertyName)
                                    Console.ResetColor()
                                    Console.Write(" to ")
                                    Console.BackgroundColor = ChosenPlayer.PlayerColour
                                    Console.Write(ChosenPlayer.username)
                                    Console.BackgroundColor = ConsoleColor.Black
                                    Console.WriteLine(" for £" & SellPrice & "?")
                                    Console.WriteLine("Enter Y for yes or N for no")
                                    choice = Console.ReadLine().ToUpper
                                    If choice = "Y" Then
                                        sold = True
                                    ElseIf choice = "N" Then
                                        Dim choice2 As String = ""
                                        Do Until choice2 = "Y" Or choice2 = "N"
                                            Console.Write("Do you want to try selling ")
                                            If Properties.GetType() = GetType(BoardProperty) Then
                                                Dim Prop As BoardProperty = Properties
                                                Dim Colour As String = Prop.Colour
                                                ColourCheck(Colour)
                                            End If
                                            Console.Write(PropertyName)
                                            Console.ResetColor()
                                            Console.WriteLine(" for a different price?")
                                            Console.WriteLine("Enter Y for yes and N for no.")
                                            choice2 = Console.ReadLine().ToUpper
                                            If choice2 = "N" Then
                                                ExitSub = True
                                            ElseIf choice2 <> "Y" Then
                                                Console.WriteLine("Input invalid.")
                                            End If
                                        Loop
                                    Else
                                        Console.WriteLine("Input invalid.")
                                    End If
                                Loop
                            Else
                                Console.WriteLine()
                                Console.Write("These players want to buy ")
                                If Properties.GetType() = GetType(BoardProperty) Then
                                    Dim Prop As BoardProperty = Properties
                                    Dim Colour As String = Prop.Colour
                                    ColourCheck(Colour)
                                End If
                                Console.Write(PropertyName)
                                Console.ResetColor()
                                Console.WriteLine(" from you for £" & SellPrice)
                                For j = 0 To PlayersWantingToBuy.Count - 1
                                    Console.BackgroundColor = PlayersWantingToBuy(j).PlayerColour
                                    Console.WriteLine(PlayersWantingToBuy(j).username)
                                    Console.BackgroundColor = ConsoleColor.Black
                                Next
                                Dim found As Boolean = False
                                Dim choice As String = ""
                                Do Until found = True Or choice = "3" Or choice = "5"
                                    Console.WriteLine()
                                    Console.Write("Enter the name of the player you want to sell ")
                                    If Properties.GetType() = GetType(BoardProperty) Then
                                        Dim Prop As BoardProperty = Properties
                                        Dim Colour As String = Prop.Colour
                                        ColourCheck(Colour)
                                    End If
                                    Console.Write(PropertyName)
                                    Console.ResetColor()
                                    Console.WriteLine(" to, 3 to try selling for another price, or enter 5 to exit.")
                                    choice = Console.ReadLine()
                                    If choice = "5" Then
                                        ExitSub = True
                                    ElseIf choice <> "3" Then
                                        For j = 0 To PlayersWantingToBuy.Count - 1
                                            If choice = PlayersWantingToBuy(j).username Then
                                                ChosenPlayer = PlayersWantingToBuy(j)
                                                found = True
                                                sold = True
                                            End If
                                        Next
                                        If found = False And ExitSub = False Then
                                            Console.WriteLine("Input invalid.")
                                        End If
                                    End If
                                Loop
                            End If
                        End If
                    Loop
                ElseIf PropertyName = "5" Then
                    valid = True
                    ExitSub = True
                End If
            Next
            If valid = False Then
                Console.WriteLine()
                Console.WriteLine("Input invalid.")
            End If
        Loop

        If sold = True Then
            ChosenPlayer.ChangeMoney(-SellPrice, False, Me)
            ChangeMoney(SellPrice, True, Nothing)
            Console.WriteLine()

            Dim ChosenProperty As SellablePosition

            For Each Square In ListOfProperties
                If PropertyName = Square.Name Then
                    ChosenProperty = Square
                    If Square.GetType() = GetType(BoardProperty) Then
                        PropertyOwnerUpdate(Square, sold, ChosenPlayer)
                    ElseIf Square.GetType() = GetType(BoardStation) Then
                        StationOwnerUpdate(Square, sold, ChosenPlayer)
                    ElseIf Square.GetType() = GetType(BoardUtility) Then
                        UtilityOwnerUpdate(Square, sold, ChosenPlayer)
                    End If
                End If
            Next

            Console.BackgroundColor = ChosenPlayer.PlayerColour
            Console.Write(ChosenPlayer.username)
            Console.BackgroundColor = ConsoleColor.Black
            Console.Write(" now owns ")
            If ChosenProperty.GetType() = GetType(BoardProperty) Then
                Dim Prop As BoardProperty = ChosenProperty
                Dim Colour As String = Prop.Colour
                ColourCheck(Colour)
            End If
            Console.WriteLine(PropertyName)
            Console.ResetColor()
        End If

    End Sub


    'NEED TO CHANGE FOR COMPUTER OPPONENT
    Sub Mortgage()

        Dim IsMortgagedProperties = (From tile As SellablePosition In Board.OfType(Of SellablePosition) Where tile.Owner Is Me And tile.IsMortgaged).ToList
        Dim NotMortgagedProperties = (From tile As SellablePosition In Board.OfType(Of SellablePosition) Where tile.CanBeMortgaged(Me) And tile.IsMortgaged = False).ToList

        Console.WriteLine()
        If IsMortgagedProperties.Count > 0 Then
            Console.WriteLine("You have currently mortgaged these properties:")
            For Each MortgagedProperty In IsMortgagedProperties
                If MortgagedProperty.GetType() = GetType(BoardProperty) Then
                    Dim Prop As BoardProperty = MortgagedProperty
                    Dim Colour As String = Prop.Colour
                    ColourCheck(Colour)
                End If
                Console.WriteLine(MortgagedProperty.Name)
            Next
            Console.ResetColor()
        Else
            Console.WriteLine("You do not have any mortgaged properties.")
        End If

        Console.WriteLine()
        Dim choice As String = ""
        Do Until choice = "1" Or choice = "2" Or choice = "3"
            Console.WriteLine()
            Console.WriteLine("Enter 1 to mortgage more properties, 2 to unmortgage properties, and 3 to exit.")
            choice = Console.ReadLine()
            If choice = "1" Then
                MortgageProperties(NotMortgagedProperties)
            ElseIf choice = "2" Then
                If IsMortgagedProperties.Count > 0 Then
                    UnmortgageProperties(IsMortgagedProperties)
                Else
                    Console.WriteLine("You do not have any properties to unmortgage.")
                End If
            ElseIf choice <> "3" Then
                Console.WriteLine("Input is invalid.")
            End If
        Loop

    End Sub

    Sub MortgageProperties(NotMortgagedProperties)

        Dim PropertyName As String
        Dim PropertyToMortgage As SellablePosition
        Dim CanMortgage As Boolean = False
        Dim choice As String = ""
        Dim valid As Boolean = False

        Console.WriteLine()
        If NotMortgagedProperties.Count > 0 Then
            CanMortgage = True
            Console.WriteLine("You can currently mortgage these properties:")
            For Each Prop In NotMortgagedProperties
                If Prop.GetType() = GetType(BoardProperty) Then
                    Dim Colour As String = Prop.Colour
                    ColourCheck(Colour)
                End If
                Console.WriteLine(Prop.Name)
            Next
            Console.ResetColor()
            Console.WriteLine()
        Else
            Console.WriteLine("You do not own any properties that can currently be mortgaged.")
        End If

        Console.WriteLine("Any other properties that you own cannot be mortgaged until all buildings on properties of that colour are sold.")

        If CanMortgage = True Then
            Do Until valid = True
                Console.WriteLine()
                Console.WriteLine("Enter the name of the property you want to mortgage, or enter 5 to exit.")

                PropertyName = Console.ReadLine().ToUpper
                For Each Prop In NotMortgagedProperties
                    If PropertyName = Prop.Name.ToUpper Then
                        PropertyToMortgage = Prop
                        valid = True
                        Exit For
                    ElseIf PropertyName = "5" Then
                        valid = True
                    End If
                Next
                If valid = False Then
                    Console.WriteLine()
                    Console.WriteLine("Input invalid.")
                    Console.WriteLine("You can currently mortgage these properties:")
                    For Each Prop In NotMortgagedProperties
                        If Prop.GetType() = GetType(BoardProperty) Then
                            Dim Colour As String = Prop.Colour
                            ColourCheck(Colour)
                        End If
                        Console.WriteLine(Prop.Name)
                    Next
                    Console.ResetColor()
                    Console.WriteLine()
                End If
            Loop

            If valid = True And PropertyName <> "5" Then
                Do Until choice = "Y" Or choice = "N"
                    Console.WriteLine()
                    Console.Write("Would you like to mortgage ")
                    If PropertyToMortgage.GetType() = GetType(BoardProperty) Then
                        Dim Prop As BoardProperty = PropertyToMortgage
                        Dim Colour As String = Prop.Colour
                        ColourCheck(Colour)
                        Console.Write(PropertyToMortgage.Name)
                        Console.ResetColor()
                        Console.WriteLine("?")
                    End If
                    Console.WriteLine("You will gain £" & PropertyToMortgage.MortgageValue)
                    Console.WriteLine("Enter Y for yes and N for no")
                    choice = Console.ReadLine().ToUpper
                    If choice = "Y" Then
                        PropertyToMortgage.IsMortgaged = True
                        ChangeMoney(PropertyToMortgage.MortgageValue, True, Nothing)
                        Console.WriteLine()
                        If PropertyToMortgage.GetType() = GetType(BoardProperty) Then
                            Dim Prop As BoardProperty = PropertyToMortgage
                            Dim Colour As String = Prop.Colour
                            ColourCheck(Colour)
                        End If
                        Console.Write(PropertyToMortgage.Name)
                        Console.ResetColor()
                        Console.WriteLine(" is now mortgaged. You cannot collect any rent on this site and you will not be able to build on any properties of this colour until you unmortgage it.")
                        Console.WriteLine("You now have £" & money)
                        Console.ReadLine()
                    End If
                Loop
            End If
        End If

    End Sub

    Sub UnmortgageProperties(IsMortgagedProperties)

        Dim PropertyName As String
        Dim PropertyToUnmortgage As SellablePosition
        Dim choice As String = ""
        Dim valid As Boolean = False
        Dim cost As Integer

        Do Until valid = True
            Console.WriteLine()
            Console.WriteLine("Enter the name of the property you want to unmortgage, or enter 5 to exit.")
            PropertyName = Console.ReadLine().ToUpper
            For Each Prop In IsMortgagedProperties
                If PropertyName = Prop.Name.ToUpper Then
                    PropertyToUnmortgage = Prop
                    cost = PropertyToUnmortgage.MortgageValue * 1.1
                    valid = True
                ElseIf PropertyName = "5" Then
                    valid = True
                End If
            Next
            If valid = False Then
                Console.WriteLine()
                Console.WriteLine("Input invalid.")
                Console.WriteLine("You can currently unmortgage these properties:")
                For j = 0 To IsMortgagedProperties.Count - 1
                    Console.WriteLine(IsMortgagedProperties(j)(0))
                Next
            End If
        Loop

        If valid = True And PropertyName <> "5" Then
            If money < cost Then
                Console.WriteLine()
                Console.WriteLine("You do not have enough money to unmortgage this property.")
                Console.WriteLine("This property costs £" & cost & " to unmortgage, but you only have £" & money)
                Console.ReadLine()
            Else
                Do Until choice = "Y" Or choice = "N"
                    Console.WriteLine()
                    Console.WriteLine("Would you like to unmortgage " & PropertyToUnmortgage.Name & " for £" & cost & "?")
                    Console.WriteLine("Enter Y for yes and N for no")
                    choice = Console.ReadLine().ToUpper
                    If choice = "Y" Then
                        PropertyToUnmortgage.IsMortgaged = False
                        ChangeMoney(-cost, True, Nothing)
                        Console.WriteLine()
                        Console.WriteLine(PropertyToUnmortgage.Name & " is now unmortgaged. You can now collect rent from this property again.")
                        Console.ReadLine()
                    End If
                Loop
            End If
        End If

    End Sub


    Sub SeeOwnedProperties()

        Console.WriteLine()
        Console.WriteLine("You currently own these properties:")
        Dim Colour As String
        For Each Square As BoardProperty In ListOfBoardProperties
            If Square.GetType() = GetType(BoardProperty) Then
                Dim Prop As BoardProperty = Square
                Colour = Prop.Colour
            End If
            If Square.Owner IsNot Nothing Then
                If Square.Owner.username = username Then
                    If Square.NumberOfHouses > 0 Then
                        ColourCheck(Colour)
                        Console.Write(Square.Name)
                        Console.ResetColor()
                        Console.Write(" - " & Square.NumberOfHouses & " houses")
                    ElseIf Square.NumberOfHotels > 0 Then
                        ColourCheck(Colour)
                        Console.Write(Square.Name)
                        Console.ResetColor()
                        Console.Write(" - " & Square.NumberOfHotels & " hotels")
                    Else
                        ColourCheck(Colour)
                        Console.Write(Square.Name)
                        Console.ResetColor()
                        Console.Write(" - no buildings")
                    End If
                    If Square.IsMortgaged = True Then
                        Console.WriteLine(" (MORTGAGED)")
                    Else
                        Console.WriteLine()
                    End If
                End If
            End If
        Next
        For Each Square As BoardStation In ListOfBoardStations
            If Square.Owner Is Me Then
                Console.Write(Square.Name)
                If Square.IsMortgaged = True Then
                    Console.WriteLine(" (MORTGAGED)")
                Else
                    Console.WriteLine()
                End If
            End If
        Next
        For Each Square As BoardUtility In ListOfBoardUtilities
            If Square.Owner Is Me Then
                Console.Write(Square.Name)
                If Square.IsMortgaged = True Then
                    Console.WriteLine(" (MORTGAGED)")
                Else
                    Console.WriteLine()
                End If
            End If
        Next

        Console.WriteLine()
        Console.WriteLine("Press enter to return to main menu")
        Console.ReadLine()

    End Sub

    Sub SeeStatusOfProperties()

        Console.WriteLine()
        Console.WriteLine("available properties have been highlighted")
        For Each Square As BoardProperty In ListOfBoardProperties
            Console.WriteLine()
            ColourCheck(Square.Colour)
            If Square.Owner Is Nothing Then
                Console.BackgroundColor = ConsoleColor.Gray
            End If
            Console.Write(Square.Name)
            Console.ResetColor()
            If Square.IsMortgaged = True Then
                Console.WriteLine(" (MORTGAGED)")
            Else
                Console.WriteLine()
            End If
            If Square.Owner IsNot Nothing Then
                Console.Write("Owner: ")
                Console.BackgroundColor = Square.Owner.PlayerColour
                Console.WriteLine(Square.Owner.username)
                Console.BackgroundColor = ConsoleColor.Black
            Else
                Console.WriteLine("Owner: ")
            End If
            If Square.NumberOfHouses > 0 Then
                Console.WriteLine("Buildings: " & Square.NumberOfHouses & " houses")
            ElseIf Square.NumberOfHotels > 0 Then
                Console.WriteLine("Buildings: " & Square.NumberOfHotels & " hotels")
            Else
                Console.WriteLine("Buildings: none")
            End If
        Next
        For Each Square As BoardStation In ListOfBoardStations
            Console.WriteLine()
            If Square.Owner Is Nothing Then
                Console.BackgroundColor = ConsoleColor.Gray
                Console.ForegroundColor = ConsoleColor.DarkGray
            End If
            Console.Write(Square.Name)
            Console.ResetColor()
            If Square.IsMortgaged = True Then
                Console.WriteLine(" (MORTGAGED)")
            Else
                Console.WriteLine()
            End If
            If Square.Owner IsNot Nothing Then
                Console.Write("Owner: ")
                Console.BackgroundColor = Square.Owner.PlayerColour
                Console.WriteLine(Square.Owner.username)
                Console.BackgroundColor = ConsoleColor.Black
            Else
                Console.WriteLine("Owner: ")
            End If
        Next
        For Each Square As BoardUtility In ListOfBoardUtilities
            Console.WriteLine()
            If Square.Owner Is Nothing Then
                Console.BackgroundColor = ConsoleColor.Gray
                Console.ForegroundColor = ConsoleColor.DarkGray
            End If
            Console.Write(Square.Name)
            Console.ResetColor()
            If Square.IsMortgaged = True Then
                Console.WriteLine(" (MORTGAGED)")
            Else
                Console.WriteLine()
            End If
            If Square.Owner IsNot Nothing Then
                Console.Write("Owner: ")
                Console.BackgroundColor = Square.Owner.PlayerColour
                Console.WriteLine(Square.Owner.username)
                Console.BackgroundColor = ConsoleColor.Black
            Else
                Console.WriteLine("Owner: ")
            End If
        Next

        Console.WriteLine()
        Console.WriteLine("Press enter to return to main menu")
        ' Console.ReadLine()

    End Sub


    Sub BuildingRepairs(HouseCost, HotelCost)

        Dim TotalCost As Integer = 0
        Dim NumberOfHouses, NumberOfHotels As Integer

        For Each Properties As BoardProperty In ListOfBoardProperties
            If Properties.Owner Is Me Then
                TotalCost = (HouseCost * Properties.NumberOfHouses) + (HotelCost * Properties.NumberOfHotels)
                NumberOfHouses += Properties.NumberOfHouses
                NumberOfHotels += Properties.NumberOfHotels
            End If
        Next

        Console.WriteLine()
        If IsHuman() Then
            Console.Write("You have ")
        Else
            Console.BackgroundColor = PlayerColour
            Console.Write(username)
            Console.BackgroundColor = ConsoleColor.Black
            Console.Write(" has ")
        End If
        Console.WriteLine("a total of " & NumberOfHouses & " houses and " & NumberOfHotels & " hotels, so the total to pay is £" & TotalCost)
        ChangeMoney(-TotalCost, True, Nothing)

    End Sub


    Function ColourCheck(Colour)

        Select Case Colour
            Case "Brown"
                Console.ForegroundColor = ConsoleColor.DarkRed
            Case "Light Blue"
                Console.ForegroundColor = ConsoleColor.DarkCyan
            Case "Pink"
                Console.ForegroundColor = ConsoleColor.Magenta
            Case "Orange"
                Console.ForegroundColor = ConsoleColor.DarkYellow
            Case "Red"
                Console.ForegroundColor = ConsoleColor.Red
            Case "Yellow"
                Console.ForegroundColor = ConsoleColor.Yellow
            Case "Green"
                Console.ForegroundColor = ConsoleColor.DarkGreen
            Case "Dark Blue"
                Console.ForegroundColor = ConsoleColor.DarkBlue
        End Select

        Return Console.ForegroundColor

    End Function


    Protected MustOverride Function DecisionBuyProperty(Square As BoardProperty, Price As Integer) As Boolean
    Protected MustOverride Function DecisionBuyStation(Square As BoardStation) As Boolean
    Protected MustOverride Function DecisionBuyUtility(Square As BoardUtility) As Boolean
    Protected MustOverride Function DecisionUseGetOutOfJailFreeCard(NumberOfGetOutOfJailCards As Integer) As Boolean
    Protected MustOverride Function DecisionPayToGetOutOfJail() As Boolean
    Protected MustOverride Sub DecisionBuyBuildings()
    Protected MustOverride Function DecisionSellBuildings() As Boolean
    Protected MustOverride Sub DecisionSell(ListOfPlayers As List(Of Player), MoneyRequired As Integer)


    Protected MustOverride Sub DecisionEndTurn()
    Protected MustOverride Function IsHuman() As Boolean

End Class