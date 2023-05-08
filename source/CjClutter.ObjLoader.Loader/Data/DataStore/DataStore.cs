using System.Collections.Generic;
using System.Linq;
using ObjLoader.Loader.Common;
using ObjLoader.Loader.Data.Elements;
using ObjLoader.Loader.Data.VertexData;

namespace ObjLoader.Loader.Data.DataStore
{
    public class DataStore : IDataStore, IGroupDataStore, IVertexDataStore, ITextureDataStore, INormalDataStore,
                             IFaceGroup, IMaterialLibrary, IElementGroup
    {
        private Group _currentGroup;

        private readonly List<Group> _groups = new List<Group>();
        private readonly SortedDictionary<string, Group> _groupsByMaterial = new SortedDictionary<string, Group>();
        private readonly List<Material> _materials = new List<Material>();

        private readonly List<Vertex> _vertices = new List<Vertex>();
        private readonly List<Texture> _textures = new List<Texture>();
        private readonly List<Normal> _normals = new List<Normal>();

        public IList<Vertex> Vertices
        {
            get { return _vertices; }
        }

        public IList<Texture> Textures
        {
            get { return _textures; }
        }

        public IList<Normal> Normals
        {
            get { return _normals; }
        }

        public IList<Material> Materials
        {
            get { return _materials; }
        }

        public IList<Group> Groups
        {
            get { return _groups; }
        }

        public void AddFace(Face face)
        {
            PushGroupIfNeeded();

            _currentGroup.AddFace(face);
        }

        public void PushGroup(string groupName)
        {
            _currentGroup = new Group(groupName);
            _groups.Add(_currentGroup);
        }


        /**
         * Called if a face or a material is encountered. 
         * Make sure we have a group even if no group was started before.
         */
        private void PushGroupIfNeeded()
        {
            if (_currentGroup == null)
            {
                PushGroup("default");
            }
        }
        

        public void AddVertex(Vertex vertex)
        {
            _vertices.Add(vertex);
        }

        public void AddTexture(Texture texture)
        {
            _textures.Add(texture);
        }

        public void AddNormal(Normal normal)
        {
            _normals.Add(normal);
        }

        public void Push(Material material)
        {
            _materials.Add(material);
        }

        public void SetMaterial(string materialName)
        {
            var material = _materials.SingleOrDefault(x => x.Name.EqualsOrdinal(materialName)) ??
                           _materials.SingleOrDefault(x => x.Name.EqualsOrdinalIgnoreCase(materialName));

            /*
             * If we have a group without a material, then assign the material to
             * the current group.
             *
             * If our current group already has a material, find/create a group that
             * has this new material.
             *
             * Note, that we might be without a group at this point.
             */

            Group existingGroup;
            bool hadGroupBefore = _groupsByMaterial.TryGetValue(material.Name, out existingGroup);
            if (hadGroupBefore)
            {
                /*
                 * If we had a group for that material before, this is a no-brainer.
                 * Just continue to use it.
                 */
                _currentGroup = existingGroup;
                return;
            }
            
            /*
             * So we didn't have a group for that material before.
             * The current group can't have the proper material, if it would, we would have
             * found it before.
             *
             * ... double check
             */
            if (_currentGroup != null)
            {
                if (_currentGroup.Material != null)
                {
                    if (_currentGroup.Material == material)
                    {
                        _groupsByMaterial[material.Name] = _currentGroup;
                        return;
                    }
                    else
                    {
                        _currentGroup = null;
                    }
                }
            }

            if (_currentGroup == null)
            {
                _currentGroup = new Group($"default ({material.Name})");
                _groups.Add(_currentGroup);   
            }
            _currentGroup.Material = material;
            _groupsByMaterial[material.Name] = _currentGroup;
        }
    }
}