namespace CsvTextEditor.Models
{
    using System;
    using Orc.ProjectManagement;

    public sealed class Project : ProjectBase, IProject, IEquatable<Project>
    {
        public Project(string location)
            : base(location)
        {
        }

        public Project(string location, string title)
            : base(location, title)
        {
        }

        public string EditorId { get; set; }

        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the code page used when writing the file back to disk.
        /// When set to 0 or -1, UTF-8 is used as default.
        /// </summary>
        public int CodePage { get; set; }

        public bool Equals(Project other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Location, other.Location);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && Equals((Project) obj);
        }

        public override int GetHashCode()
        {
            return (Location is not null ? Location.GetHashCode() : 0);
        }

        public void SetIsDirty(bool isDirty)
        {
            if (isDirty)
            {
                MarkAsDirty();
            }
            else
            {
                ClearIsDirty();
            }
        }
    }
}
