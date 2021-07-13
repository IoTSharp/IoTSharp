import { shallowRef, ref, onMounted } from 'vue';
import { Graph } from '@antv/x6';
import '@antv/x6-vue3-shape';
import Comp from './devicegraph.vue';

export default function useGraph() {
  const container = ref<HTMLElement | null>(null);
  const graph = shallowRef<Graph | null>();
  onMounted(() => {
    if (container.value) {
      graph.value = new Graph({
        container: container.value,
        panning: true,
      });
      graph.value.addNode({
        id: 'node1',
        x: 40,
        y: 40,
        width: 100,
        height: 40,
        shape: 'vue3-shape',
        // here are 4 ways usages:
        // 1. component: Comp
        // 2. component: <Comp />
        // 3. component: () => <Comp />
        // 4. component: 'text node'
        component: Comp,
      });
    }
  });
  return {
    container,
    graph,
  };
}
