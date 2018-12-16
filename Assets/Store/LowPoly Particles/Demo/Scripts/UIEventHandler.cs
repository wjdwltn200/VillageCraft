namespace MoenenGames.LowpolyParticle {

	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;
	using UnityEngine.EventSystems;
	using UnityEngine.Events;


	public class UIEventHandler : Selectable, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {



		[System.Serializable]
		public class PointerEvent : UnityEvent<PointerEventData> { }



		public PointerEvent OnClick = null;
		public PointerEvent OnEnter = null;
		public PointerEvent OnExit = null;
		public PointerEvent OnPress = null;
		public PointerEvent OnRelease = null;



		protected override void OnDisable () {
			base.OnDisable();
			OnExit.Invoke(null);
			OnRelease.Invoke(null);
			StopAllCoroutines();
		}


		protected override void OnDestroy () {
			base.OnDestroy();
			OnClick.RemoveAllListeners();
			OnEnter.RemoveAllListeners();
			OnExit.RemoveAllListeners();
			OnPress.RemoveAllListeners();
			OnRelease.RemoveAllListeners();
			StopAllCoroutines();
		}



		public void OnPointerClick (PointerEventData eventData) {
			OnClick.Invoke(eventData);
		}


		public override void OnPointerEnter (PointerEventData eventData) {
			base.OnPointerEnter(eventData);
			OnEnter.Invoke(eventData);
		}


		public override void OnPointerExit (PointerEventData eventData) {
			base.OnPointerExit(eventData);
			OnExit.Invoke(eventData);
		}


		public override void OnPointerDown (PointerEventData eventData) {
			base.OnPointerDown(eventData);
			OnPress.Invoke(eventData);
		}


		public override void OnPointerUp (PointerEventData eventData) {
			base.OnPointerUp(eventData);
			OnRelease.Invoke(eventData);
		}


	}
}