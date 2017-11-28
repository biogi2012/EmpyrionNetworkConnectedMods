﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCode
{
    public class Entity : IEquatable<Entity>
    {
        public string Name { get; protected set; }

        public WorldPosition Position { get; protected set; }

        public Faction MemberOfFaction { get; protected set; }

        public Task Teleport(Vector3 newPosition, Vector3 newRotation)
        {
            return _gameServerConnection.SendRequest(
                Eleon.Modding.CmdId.Request_Entity_Teleport,
                new Eleon.Modding.IdPositionRotation(EntityId, newPosition, newRotation));
        }

        public Task ChangePlayfield(WorldPosition newWorldPosition)
        {
            return _gameServerConnection.SendRequest(
                Eleon.Modding.CmdId.Request_Entity_ChangePlayfield,
                new Eleon.Modding.IdPlayfieldPositionRotation(EntityId, newWorldPosition.playfield.Name, newWorldPosition.position, newWorldPosition.rotation));
        }

        public Task Destroy()
        {
            return _gameServerConnection.SendRequest(Eleon.Modding.CmdId.Request_Entity_Destroy, new Eleon.Modding.Id(EntityId));
        }

        //Request_Entity_Destroy2,            // IdPlayfield (id of entity, playfield the entity is in)
        //Request_Entity_Export,              // EntityExportInfo
        //Request_Entity_SetName,             // IdPlayfieldName (if playfield == null we try to find the corresponding playfield, playfield must be loaded)



        public Task GetCurrentPosition()
        {
            return _gameServerConnection.SendRequest<Eleon.Modding.IdPositionRotation>(Eleon.Modding.CmdId.Request_Entity_PosAndRot, new Eleon.Modding.Id(EntityId))
                .ContinueWith((task) =>
                {
                    var newData = task.Result;

                    lock (this)
                    {
                        Position = new WorldPosition(Position.playfield, new Vector3(newData.pos), new Vector3(newData.rot));
                    }
                });
        }

        #region Internal Methods

        internal void UpdateInfo(Playfield playfield)
        {
            this.Position = new WorldPosition(playfield, this.Position.position, this.Position.rotation);
        }

        internal int EntityId
        {
            get
            {
                return _entityId;
            }
        }

        #endregion

        #region Protected methods

        protected Entity(GameServerConnection gameServerConnection, int entityId, string name)
        {
            _gameServerConnection = gameServerConnection;
            _entityId = entityId;
            Name = name;
        }

        #endregion

        #region Protected Data

        protected GameServerConnection _gameServerConnection;

        #endregion

        #region Private Data

        private int _entityId;

        #endregion

        #region Common overloads

        public override string ToString()
        {
            return string.Format("{0}({1}", Name, EntityId );
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Entity e = obj as Entity;
            if (e == null)
            {
                return false;
            }

            return (_entityId == e._entityId);
        }

        public bool Equals(Entity other)
        {
            return other != null &&
                   _entityId == other._entityId;
        }

        public override int GetHashCode()
        {
            return -1485059848 + _entityId.GetHashCode();
        }

        public static bool operator ==(Entity entity1, Entity entity2)
        {
            return EqualityComparer<Entity>.Default.Equals(entity1, entity2);
        }

        public static bool operator !=(Entity entity1, Entity entity2)
        {
            return !(entity1 == entity2);
        }

        #endregion
    }
}