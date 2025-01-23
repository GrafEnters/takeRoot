using System;
using System.Collections.Generic;
using UnityEngine;

namespace WorldObjects
{
    public class TacticalInteractable : ECSComponent, ISelectable
    {
        [SerializeField]
        private Vector2Int _interactableShift;
        public Gridable Gridable { get; private set; }
        public Func<InfoBookData> GetInfoFunc;
        
        
        public TacticalCommandData CommandToExecute { get; private set; }
        private readonly List<TacticalCommand> _availableCommands = new();
        
        public Action<TacticalCommand> OnCommandPerformed, OnCommandCanceled;
        
        public Vector2Int GetInteractableCell => Gridable.GetBottomLeftOnGrid + _interactableShift;

        public bool CanBeCommanded(TacticalCommand command)
        {
            if (CommandToExecute != null && CommandToExecute.TacticalCommandType == command) {
                return false;
            }
            if (CommandToExecute != null && command == TacticalCommand.Cancel) {
                return true;
            }

            return _availableCommands.Contains(command);
        }
        
        public void RemoveFromPossibleCommands(TacticalCommand command) {
            if (!_availableCommands.Contains(command)) {
                return;
            }
            
            _availableCommands.Remove(command);
        }

        public void AddToPossibleCommands(TacticalCommand command) {
            if (_availableCommands.Contains(command)) {
                return;
            }

            _availableCommands.Add(command);
        }

        public void AssignCommand(TacticalCommandData command) {
            CommandToExecute = command;
        }

        public void CancelCommand() {
            if (CommandToExecute == null) {
                return;
            }

            OnCommandCanceled?.Invoke(CommandToExecute.TacticalCommandType);
            CommandToExecute = null;
        }

        public void ExecuteCommand() {
            if (CommandToExecute == null) {
                return;
            }

            OnCommandPerformed?.Invoke(CommandToExecute.TacticalCommandType);
        }

        private void OnMouseEnter() {
            SelectionManager.Instance.SetTacticalSelected(this);
        }

        private void OnMouseExit() {
            SelectionManager.Instance.TryClearTacticalSelected(this);
        }
        
        public void OnDestroyed() {
            _availableCommands.Clear();
            CommandToExecute?.TriggerCancel?.Invoke();
            CancelCommand();
            Destroy(gameObject);
        }
        
        public override int GetDependancyPriority()
        {
            return 0;
        }

        public override void Init(ECSEntity entity)
        {
            Gridable = entity.GetEcsComponent<Gridable>();
        }

        public InfoBookData GetInfoData()
        {
            return GetInfoFunc?.Invoke();
        }
    }
}