﻿Imports System.Configuration
Imports System.Data.Linq

Namespace Data

  ''' <summary>
  ''' Provides an automated way to reuse 
  ''' LINQ data context objects within the context
  ''' of a single data portal operation.
  ''' </summary>
  ''' <typeparam name="C">
  ''' Type of database 
  ''' LINQ data context objects object to use.
  ''' </typeparam>
  ''' <remarks>
  ''' This type stores the LINQ data context object 
  ''' in <see cref="Csla.ApplicationContext.LocalContext" />
  ''' and uses reference counting through
  ''' <see cref="IDisposable" /> to keep the data context object
  ''' open for reuse by child objects, and to automatically
  ''' dispose the object when the last consumer
  ''' has called Dispose."
  ''' </remarks>
  Public Class ContextManager(Of C As DataContext)

    Implements IDisposable

    Private Shared _lock As New Object
    Private _context As C
    Private _connectionString As String

    ''' <summary>
    ''' Gets the ContextManager object for the specified
    ''' key name.
    ''' </summary>
    ''' <param name="databaseName">
    ''' The database name.
    ''' </param>
    ''' <param name="getConnectionString">
    ''' True to get the connection string from
    ''' the config file. False to treat the
    ''' database name as the connection string.
    ''' </param>
    ''' <returns>ContextManager object for the name.</returns>
    Public Shared Function GetManager(ByVal databaseName As String, ByVal getConnectionString As Boolean) As ContextManager(Of C)

      If getConnectionString Then
        Return GetManager(ConfigurationManager.ConnectionStrings(databaseName).ConnectionString)

      Else
        Return GetManager(databaseName)
      End If

    End Function

    ''' <summary>
    ''' Gets the ContextManager object for the specified
    ''' key name.
    ''' </summary>
    ''' <param name="connectionString">
    ''' The database connection string.
    ''' </param>
    ''' <returns>ContextManager object for the name.</returns>
    Public Shared Function GetManager(ByVal connectionString As String) As ContextManager(Of C)

      SyncLock _lock
        Dim mgr As ContextManager(Of C)
        If ApplicationContext.LocalContext.Contains("__ctx:" & connectionString) Then
          mgr = CType(ApplicationContext.LocalContext("__ctx:" & connectionString), ContextManager(Of C))

        Else
          mgr = New ContextManager(Of C)(connectionString)
          ApplicationContext.LocalContext("__ctx:" & connectionString) = mgr
        End If
        mgr.AddRef()
        Return mgr
      End SyncLock

    End Function

    Private Sub New(ByVal connectionString As String)

      _connectionString = connectionString

      _context = DirectCast(Activator.CreateInstance(GetType(C), connectionString), C)

    End Sub

    ''' <summary>
    ''' Gets the LINQ data context object.
    ''' </summary>
    Public ReadOnly Property DataContext() As C
      Get
        Return _context
      End Get
    End Property

#Region " Reference counting "

    Private _refCount As Integer

    Private Sub AddRef()
      _refCount += 1
    End Sub

    Private Sub DeRef()

      SyncLock _lock
        _refCount -= 1
        If _refCount = 0 Then
          _context.Dispose()
          ApplicationContext.LocalContext.Remove("__ctx:" & _connectionString)
        End If
      End SyncLock

    End Sub

#End Region

#Region " IDisposable "

    ''' <summary>
    ''' Dispose object, dereferencing or
    ''' disposing the context it is
    ''' managing.
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
      DeRef()
    End Sub

#End Region

  End Class

End Namespace