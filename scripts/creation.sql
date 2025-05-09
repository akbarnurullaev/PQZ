CREATE TABLE [PotatoTeacher] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    PRIMARY KEY ([Id])
);

CREATE TABLE [Quiz] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [PotatoTeacherId] int NOT NULL,
    [PathFile] nvarchar(255) NOT NULL,
    PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Quiz_PotatoTeacher] FOREIGN KEY ([PotatoTeacherId]) 
        REFERENCES [PotatoTeacher]([Id])
);
