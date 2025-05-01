using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ModelLayer.Entity;

namespace RepositoryLayer.Context
{
    public class UserContext:DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options) { }

        public DbSet<UserEntity> Users { get; set; }

        public DbSet<NotesEntity> Notes { get; set; }

        public DbSet<LabelEntity> Labels { get; set; }

        public DbSet<NoteLabelEntity> NoteLabels { get; set; }
        public DbSet<CollaboratorEntity> Collaborators { get; set; }

    }
}
