import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { NzTreeFlatDataSource, NzTreeFlattener } from 'ng-zorro-antd/tree-view';
import { SelectionModel } from '@angular/cdk/collections';
import { FlatTreeControl } from '@angular/cdk/tree';
import { NzDrawerRef } from 'ng-zorro-antd/drawer';
@Component({
  selector: 'app-conditionbuilder',
  templateUrl: './conditionbuilder.component.html',
  styleUrls: ['./conditionbuilder.component.less']
})
export class ConditionbuilderComponent implements OnInit {
  treeData: TreeNode[] = [
    {
      name: '1',
      key: '1',
      children: []
    }
  ];
  columns = [

  ];
  private transformer = (node: TreeNode, level: number): FlatNode => {
    const existingNode = this.nestedNodeMap.get(node);
    const flatNode =
      existingNode && existingNode.key === node.key
        ? existingNode
        : {
            expandable: true,
            name: node.name,
            level,
            key: node.key
          };
    flatNode.name = node.name;
    this.flatNodeMap.set(flatNode, node);
    this.nestedNodeMap.set(node, flatNode);
    return flatNode;
  };

  flatNodeMap = new Map<FlatNode, TreeNode>();
  nestedNodeMap = new Map<TreeNode, FlatNode>();
  selectListSelection = new SelectionModel<FlatNode>(true);

  treeControl = new FlatTreeControl<FlatNode>(
    node => node.level,
    node => node.expandable
  );
  treeFlattener = new NzTreeFlattener(
    this.transformer,
    node => node.level,
    node => node.expandable,
    node => node.children
  );

  dataSource = new NzTreeFlatDataSource(this.treeControl, this.treeFlattener);

  constructor(private drawerRef: NzDrawerRef<string>) {
    this.dataSource.setData(this.treeData);
    this.treeControl.expandAll();
  }

  Json = '{"a":1,"b":2,"c":3}';
  ngModelChange($event) {
    var data = JSON.parse($event);
    this.columns = [];
    for (let key in data) {
      this.columns.push({ label: key, value: key });
    }
  }
  ngOnInit(): void {
    var data = JSON.parse(this.Json);
    this.columns = [];
    for (let key in data) {
      this.columns.push({ label: key, value: key });
    }
  }

  hasChild = (_: number, node: FlatNode): boolean => true;
  hasNoContent = (_: number, node: FlatNode): boolean => node.name === '';
  trackBy = (_: number, node: FlatNode): string => `${node.key}-${node.name}`;
  close(){
    this.drawerRef.close({expression:this.expression,orgin:this.treeData})
  }
  delete(node: FlatNode): void {
    const originNode = this.flatNodeMap.get(node);

    const dfsParentNode = (): TreeNode | null => {
      const stack = [...this.treeData];
      while (stack.length > 0) {
        const n = stack.pop()!;
        if (n.children) {
          if (n.children.find(e => e === originNode)) {
            return n;
          }

          for (let i = n.children.length - 1; i >= 0; i--) {
            stack.push(n.children[i]);
          }
        }
      }
      return null;
    };

    const parentNode = dfsParentNode();
    if (parentNode && parentNode.children) {
      parentNode.children = parentNode.children.filter(e => e !== originNode);
    }

    this.dataSource.setData(this.treeData);
  }
  addNewNode(node: FlatNode): void {
    const parentNode = this.flatNodeMap.get(node);
    if (parentNode) {
      parentNode.children = parentNode.children || [];
      parentNode.children.push({
        name: node.field,
        key: `${parentNode.key}-${parentNode.children.length}`,
        children: []
      });

      this.dataSource.setData(this.treeData);
      this.treeControl.expand(node);
    }
  }

  saveNode(node: FlatNode, value: string): void {
    const nestedNode = this.flatNodeMap.get(node);
    if (nestedNode) {
      nestedNode.name = value;
      this.dataSource.setData(this.treeData);
    }
  }
  create() {
    this.expression = this.rebuildtree(this.treeData);
  }

  expression: string = '';

  rebuildtree(nodes: TreeNode[]) {
    var expression = '';
    for (var i = 0; i < nodes.length; i++) {
      if (i == nodes.length - 1) {
        var node = nodes[i];
        if (node.children && node.children?.length > 0) {
          var flatNode = this.nestedNodeMap.get(node);
          node.connector = flatNode.connector;
          node.field = flatNode.field;
          node.text = flatNode.text;
          node.operator = flatNode.operator;
          node.name = flatNode.name;
          if (node.children && node.children?.length > 0) {
            this.rebuildtree(node.children);
            expression += node.field + ' ' + node.operator + '' + node.text + node.connector + '(' + this.rebuildtree(node.children) + ')';
          } else {
            expression += node.field + ' ' + node.operator + '' + node.text + node.connector;
          }
        } else {
          var node = nodes[i];
          var flatNode = this.nestedNodeMap.get(node);
          node.connector = flatNode.connector;
          node.field = flatNode.field;
          node.text = flatNode.text;
          node.operator = flatNode.operator;
          node.name = flatNode.name;
          expression += node.field + ' ' + node.operator + '' + node.text;
        }
      } else {
        var node = nodes[i];
        var flatNode = this.nestedNodeMap.get(node);
        node.connector = flatNode.connector;
        node.field = flatNode.field;
        node.text = flatNode.text;
        node.operator = flatNode.operator;
        node.name = flatNode.name;
        if (node.children && node.children?.length > 0) {
          expression += node.field + ' ' + node.operator + '' + node.text + node.connector + '(' + this.rebuildtree(node.children) + ')';
        } else {
          expression += node.field + ' ' + node.operator + '' + node.text + node.connector;
        }
      }
    }
    return expression;
  }
}
interface TreeNode {
  name: string;
  key: string;
  children?: TreeNode[];
  level?: number;
  operator?: string;
  field?: string;
  text?: string;
  connector?: string;
}

interface FlatNode {
  expandable: boolean;
  name: string;
  key: string;
  level: number;
  operator?: string;
  field?: string;
  text?: string;
  connector?: string;
}
