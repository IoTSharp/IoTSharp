import { SelectionModel } from '@angular/cdk/collections';
import { FlatTreeControl } from '@angular/cdk/tree';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { NzTreeFlattener, NzTreeFlatDataSource } from 'ng-zorro-antd/tree-view';

@Component({
  selector: 'app-conditionbuilder',
  templateUrl: './conditionbuilder.component.html',
  styleUrls: ['./conditionbuilder.component.less']
})
export class ConditionbuilderComponent implements OnInit {

  columns = [{ label: 'columna', value: 'columna' }, { label: 'columnb', value: 'columnb' }, { label: 'columnc', value: 'columnc' }, { label: 'columnd', value: 'columnd' }];

  TREE_DATA: TreeNode[] = [
    {
      name: '1',
      key: '1', level: 0,
      children: [
        {
          name: '1-0',
          key: '1-0', level: 1
        }
      ]

      // children: [
      //   {
      //     name: 'parent 1-0',
      //     key: '1-0',
      //     children: [
      //       {
      //         name: 'leaf', key: '1-0-0', children: [
      //           { name: 'leaf', key: '1-0-0' },

      //         ]
      //       },
      //       { name: 'leaf', key: '1-0-1' }
      //     ]
      //   },
      //   {
      //     name: 'parent 1-1',
      //     key: '1-1',
      //     children: [{ name: 'leaf', key: '1-1-0' }]
      //   }
      // ]
    },

  ];

  private transformer = (node: TreeNode, level: number) => {
    const existingNode = this.nestedNodeMap.get(node);
    const flatNode =
      existingNode && existingNode.key === node.key
        ? existingNode
        : {
          expandable: !!node.children && node.children.length > 0,
          name: node.name,
          level,
          key: node.key
        };
    flatNode.name = node.name;
    this.flatNodeMap.set(flatNode, node);
    this.nestedNodeMap.set(node, flatNode);
    return flatNode;
  };

  treeData = this.TREE_DATA;
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

  constructor(private cd: ChangeDetectorRef) {
    this.dataSource.setData(this.treeData);
    this.treeControl.expandAll();


  }

  hasChild = (_: number, node: FlatNode) => node.expandable;

  hasNoContent = (_: number, node: FlatNode) => node.name === '';
  trackBy = (_: number, node: FlatNode) => `${node.key}-${node.name}`;

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
    this.dataSource = new NzTreeFlatDataSource(this.treeControl, this.treeFlattener);
    this.dataSource.setData(this.treeData);
  }
  addNewNode(flatNode: FlatNode): void {
    var node = this.flatNodeMap.get(flatNode);
    if (node) {



      var _node = {
        name: `${node.key}-${node.children.length}`,
        key: `${node.key}-${node.children.length}`,
        level: flatNode.level + 1, children: []

      };



      // if (node.children && node.children[0] && node.children[0].key.endsWith('-0')) {

   
      //   var tmp = node.children[0]
      //   var tmpflat = this.nestedNodeMap.get(tmp)

      //   console.log(this.flatNodeMap.has(tmpflat))
      //   console.log(this.nestedNodeMap.has(tmp))
      //   console.log(this.flatNodeMap.delete(tmpflat))
      //   console.log(this.nestedNodeMap.delete(tmp))
      //   node.children = node.children.splice(0, 1)
      //   console.log(tmpflat)
      //   console.log(tmp)
      //   this.dataSource.setData(this.treeData);

      //   console.log(this.flatNodeMap)
      //   console.log(this.nestedNodeMap)
      // }




      _node.children = [
        {
          name: `${_node.key}-0`,
          key: `${_node.key}-0`,
          level: flatNode.level + 2, children: []
        }
      ]
      node.children.push(_node);
      this.dataSource.setData(this.treeData);
      this.treeControl.expand(flatNode);
      this.flatNodeMap.set(this.transformer(_node, flatNode.level + 1), _node)
      this.nestedNodeMap.set(_node, this.transformer(_node, flatNode.level + 1))


      this.dataSource.setData(this.treeData);
      this.treeControl.expand(flatNode);
  

    }

    this.cd.detectChanges();

  }

  saveNode(node: FlatNode, value: string): void {
    const nestedNode = this.flatNodeMap.get(node);
    if (nestedNode) {
      nestedNode.name = value;
      this.dataSource.setData(this.treeData);
    }
  }

  ngOnInit(): void {
  }

}

interface TreeNode {
  name: string;
  key: string;
  children?: TreeNode[];
  level: number;
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
