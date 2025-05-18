using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.AutoGrouper.Models
{
    internal sealed class TypeDiagnosticNode
    {
        private readonly List<PropertyDiagnosticNode> properties = [];
        private readonly List<PropertyDiagnosticNode> parents = [];

        public string TypeName { get; }
        public IReadOnlyList<PropertyDiagnosticNode> Properties => properties.AsReadOnly();
        public IReadOnlyList<PropertyDiagnosticNode> Parents => parents.AsReadOnly();

        public TypeDiagnosticNode(string typeName)
        {
            TypeName = typeName;
        }

        public void AddProperty(PropertyDiagnosticNode property)
        {
            ArgumentNullException.ThrowIfNull(property);
            properties.Add(property);
            property.AddParent(this);
        }

        public override bool Equals(object? obj)
        {
            if (obj is TypeDiagnosticNode other)
            {
                return TypeName == other.TypeName;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return TypeName.GetHashCode();
        }
    }
}
