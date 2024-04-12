using Codice.CM.Common.Tree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.GraphView;

public class BehaviorTreeView : GraphView
{
	public new class UxmlFactory : UxmlFactory<BehaviorTreeView, GraphView.UxmlTraits> { }
	
	BehaviorTree _tree;

	public BehaviorTreeView()
	{
		Insert(0, new GridBackground());

		this.AddManipulator(new ContentZoomer());
		this.AddManipulator(new ContentDragger());
		this.AddManipulator(new SelectionDragger());
		this.AddManipulator(new RectangleSelector());

		var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/BehaviorTreeEditor/BehaviorTreeEditor.uss");
		styleSheets.Add(styleSheet);
	}

	NodeView FindNodeView(Node node)
	{
		return GetNodeByGuid(node.guid) as NodeView;
	}

	internal void PopulateView(BehaviorTree tree)
	{
		_tree = tree;

		graphViewChanged -= OnGraphViewChanged;
		DeleteElements(graphElements);
		graphViewChanged += OnGraphViewChanged;

		// Create NodeView
		tree.nodes.ForEach(n => CreateNodeView(n));

        // Create Edge
        tree.nodes.ForEach(n => {
			List<Node> children = tree.GetChildren(n);
			children.ForEach(c =>
			{
				NodeView parentView = FindNodeView(n);
				NodeView childView = FindNodeView(c);

				Edge edge = parentView.output.ConnectTo(childView.input);
				AddElement(edge);
			});
		});
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
		return ports.ToList().Where(endport =>
		endport.direction != startPort.direction &&
		endport.node != startPort.node).ToList();
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
	{
		if (graphViewChange.elementsToRemove != null)
		{
			graphViewChange.elementsToRemove.ForEach(elem =>
			{
				NodeView nodeView = elem as NodeView;
				if (nodeView != null)
				{
					_tree.DeleteNode(nodeView.node);
				}

				Edge edge = elem as Edge;
                if (edge != null)
                {
                    NodeView parentView = edge.output.node as NodeView;
                    NodeView childView = edge.input.node as NodeView;
                    _tree.RemoveChild(parentView.node, childView.node);
                }
            });
		}

		if (graphViewChange.edgesToCreate != null)
		{
			graphViewChange.edgesToCreate.ForEach(edge =>
			{
				NodeView parentView = edge.output.node as NodeView;
				NodeView childView = edge.input.node as NodeView;
				_tree.AddChild(parentView.node, childView.node);
			});
		}

		return graphViewChange;
	}

	public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
	{
		{
			var types = TypeCache.GetTypesDerivedFrom<ActionNode>();
			foreach (var type in types)
			{
				evt.menu.AppendAction($"[{type.BaseType.Name}] {type.Name}", (a) => CreateNode(type));
			}
		}
		{
			var types = TypeCache.GetTypesDerivedFrom<CompositeNode>();
			foreach (var type in types)
			{
				evt.menu.AppendAction($"[{type.BaseType.Name}] {type.Name}", (a) => CreateNode(type));
			}
		}
		{
			var types = TypeCache.GetTypesDerivedFrom<DecoratorNode>();
			foreach (var type in types)
			{
				evt.menu.AppendAction($"[{type.BaseType.Name}] {type.Name}", (a) => CreateNode(type));
			}
		}
	}

	void CreateNode(System.Type type)
	{
		Node node = _tree.CreateNode(type);
		CreateNodeView(node);
	}

	void CreateNodeView(Node node)
	{
		NodeView nodeView = new NodeView(node);
		AddElement(nodeView);
	}
}