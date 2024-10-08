using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeView : UnityEditor.Experimental.GraphView.Node
{
	public Action<NodeView> onNodeSelected;
	public Node node;
	public Port input;
	public Port output;

	public NodeView(Node node) : base("Assets/Data/UIBuilder/NodeView.uxml")
	{
		this.node = node;
		this.title = node.name;
		this.viewDataKey = node.guid;

		style.left = node.position.x;
		style.top = node.position.y;

		CreateInputPorts();
		CreateOutputPorts();
		SetupClasses();

		Label descriptionLabel = this.Q<Label>("description");
		descriptionLabel.bindingPath = "description";
		descriptionLabel.Bind(new SerializedObject(node));
	}

	private void SetupClasses()
	{
		if (node is ActionNode)
		{
			AddToClassList("action");
		}
		else if (node is CompositeNode)
		{
			AddToClassList("composite");
		}
		else if (node is DecoratorNode)
		{
			AddToClassList("decorator");
		}
		else if (node is RootNode)
		{
			AddToClassList("root");
		}
	}

	private void CreateOutputPorts()
	{
		if (node is ActionNode)
		{
            input = base.InstantiatePort(Orientation.Vertical, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Single, typeof(bool));
		}
		else if (node is CompositeNode)
		{
            input = base.InstantiatePort(Orientation.Vertical, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Single, typeof(bool));
		}
		else if (node is DecoratorNode)
		{
            input = base.InstantiatePort(Orientation.Vertical, UnityEditor.Experimental.GraphView.Direction.Input, Port.Capacity.Single, typeof(bool));
		}
		else if (node is RootNode)
		{
		}

		if (input != null)
		{
			input.portName = "";
			input.style.flexDirection = FlexDirection.Column;
			inputContainer.Add(input);
		}
	}

	private void CreateInputPorts()
	{
		if (node is ActionNode)
		{
		}
		else if (node is CompositeNode)
		{
            output = base.InstantiatePort(Orientation.Vertical, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Multi, typeof(bool));
		}
		else if (node is DecoratorNode)
		{
            output = base.InstantiatePort(Orientation.Vertical, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(bool));
		}
		else if (node is RootNode)
		{
            output = base.InstantiatePort(Orientation.Vertical, UnityEditor.Experimental.GraphView.Direction.Output, Port.Capacity.Single, typeof(bool));
		}

		if (output != null)
		{
			output.portName = "";
			output.style.flexDirection = FlexDirection.ColumnReverse;
			outputContainer.Add(output);
		}
	}

	public override void SetPosition(Rect newPos)
	{
		base.SetPosition(newPos);
		Undo.RecordObject(node, "Behavior Tree (Set Position)");
		node.position.x = newPos.xMin;
		node.position.y = newPos.yMin;
		EditorUtility.SetDirty(node);
	}

	public override void OnSelected()
	{
		base.OnSelected();
		if (onNodeSelected != null)
		{
			onNodeSelected.Invoke(this);
		}	
	}

	public void SortChildren()
	{
		CompositeNode composite = node as CompositeNode;
		if (composite)
		{
			composite.children.Sort(SortByHorizontalPosition);
		}
	}

	private int SortByHorizontalPosition(Node left, Node right)
	{
		return left.position.x < right.position.x ? -1 : 1;
	}

	public void UpdateState()
	{
		RemoveFromClassList("running");
		RemoveFromClassList("success");
		RemoveFromClassList("failure");

		if (Application.isPlaying)
		{
			switch (node.state)
			{
			case Node.State.Running:
				if (node.started)
				{
					AddToClassList("running");
				}
				break;
			case Node.State.Success:
				AddToClassList("success");
				break;
			case Node.State.Failure:
				AddToClassList("failure");
				break;
			}
		}
	}
}