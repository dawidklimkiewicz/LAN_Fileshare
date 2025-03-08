﻿using System;

namespace LAN_Fileshare.Models
{
    public interface IFile
    {
        FileState FileState { get; set; }
        Guid Id { get; set; }
        string Name { get; set; }
        long Size { get; set; }
        DateTime TimeCreated { get; set; }
        DateTime? TimeFinished { get; set; }
    }
}