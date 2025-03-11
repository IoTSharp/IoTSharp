<template>
	<div>
		<div ref="webgl" class="three-container">
			<!-- 
        <div ref="label" class="testlabel" @click="clickme">点我!</div> -->
		</div>

		<div class="dev-panel" ref="devpanel" v-show="state.panelvisable">
			<el-card class="dev-card">
				<template #header>
					<div class="card-header">
						<span>设备</span> <el-button type="danger" :icon="CloseBold" circle size="small" style="float: right" @click="panelclose" />
					</div>
				</template>
				<el-collapse v-model="state.activename" accordion>
					<el-collapse-item title="设备属性" name="1">
						<el-descriptions column="1" size="small" direction="horizontal" style="background: transparent" label-width="60px">
							<!-- <el-descriptions-item :label="command[0]" v-for="(command, index) in state.propsdata">{{ command[1] }}</el-descriptions-item> -->
						</el-descriptions>
					</el-collapse-item>
					<el-collapse-item title="设备当前状态" name="2"> </el-collapse-item>
					<el-collapse-item title="设备历史状态" name="3"> </el-collapse-item>
					<el-collapse-item title="操作" name="4">
						<div v-for="(item, index) in state.commands">
							<span>{{ item.commandtext }}</span>
							<el-button type="danger" size="small" @click="excutecommand(state.bizId, item.commandaction)">{{ item.commandtext }}</el-button>
						</div>
					</el-collapse-item>
				</el-collapse>
			</el-card>
		</div>
	</div>
</template>

<script setup lang="ts" name="scene">
import { onMounted, reactive, ref } from 'vue';
import * as THREE from 'three';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls.js';
import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader.js';
import { DRACOLoader } from 'three/examples/jsm/loaders/DRACOLoader.js';
import { RoomEnvironment } from 'three/examples/jsm/environments/RoomEnvironment.js';
import { CSS3DObject, CSS3DRenderer } from 'three/examples/jsm/renderers/CSS3DRenderer.js';
import { Water } from 'three/addons/objects/Water.js';
import { Sky } from 'three/addons/objects/Sky.js';
import { Stats } from 'three/examples/jsm/libs/stats.module.js';
import { ElMessageBox } from 'element-plus';


interface stateobject {
	panelvisable: boolean;
	activename: string;
	bizId: string | number;
	propsdata: Map<string, string>;
	commands: Array<commanditem>;
}

interface csstextobject {
	x: number;
	y: number;
	z: number;
	text: string;
	r: number;
	bizId: string;
}
interface spriteobject {
	x: number;
	y: number;
	z: number;
	text: string;
	r: number;
}
interface cubeobject {
	width: number;
	height: number;
	depth: number;
	x: number;
	y: number;
	z: number;
	r: number;
	maxheight: number;
	minheight: number;
	step: number;
	currentacton: string;
	cubu: any;
	color: number;
	bizId: number | string;
}

interface commanditem {
	commandtext: string;
	commandaction: string;
}
interface waterobject {
	width: number;
	height: number;
	x: number;
	y: number;
	z: number;
	rx: number;
	ry: number;
	rz: number;
	maxheight: number;
	minheight: number;
	currentacton: string;
	water: any;
}
const state = reactive<stateobject>({
	panelvisable: false,
	activename: '1',

	bizId: '',
	commands: [
		{
			commandtext: 'open',
			commandaction: 'open',
		},
		{
			commandtext: 'close',
			commandaction: 'close',
		},
	],
	propsdata: undefined,
});
const csstextobjects: Array<csstextobject> = [
	{
		x: -20.5,
		y: 5.5,
		z: -8,
		text: 'device1',
		r: -Math.PI / 2,
		bizId: '1179',
	},
	{
		x: -20.5,
		y: 5.5,
		z: 20,
		text: 'device2',
		r: 0,
		bizId: '1180',
	},
];
const spriteobjects: Array<spriteobject> = [
	{
		x: 20,
		y: 12,
		z: 1,
		text: '2D spriteobject。。。',
		r: 10,
	},
	{
		x: 20,
		y: 10,
		z: 1,
		text: '2D spriteobject。。。',
		r: 10,
	},
];

const cubeobjects: Array<cubeobject> = [
	{
		width: 40,
		height: 30,
		depth: 3.2,
		x: 0,
		y: -20,
		z: -50,
		r: Math.PI / 6,
		maxheight: 0,
		minheight: 0,
		step: 0.002,
		currentacton: '',
		cubu: {},
		color: 0xaf4feb,
		bizId: '1111',
	},
];

const waters: Array<waterobject> = [
	{
		width: 1000,
		height: 1000,
		x: 8,
		y: 0,
		z: 30,
		rx: -Math.PI / 2,
		ry: 0,
		rz: 0,
		maxheight: 0,
		minheight: 0,
		currentacton: '',
		water: {},
	},
];

const labels: Array<any> = [];

const webgl = ref();

const devpanel = ref();
const excutecommand = (id: string | number, action: string) => {


ElMessage.success('Device'+id+'excute command'+action);
};

const panelclose = () => {
	state.panelvisable = false;
};

onMounted(() => {
	const rect = webgl.value.getBoundingClientRect();
	let W = window.innerWidth;
	let H = window.innerHeight;
	const raycaster = new THREE.Raycaster();
	const mouse = new THREE.Vector2();
	const sun = new THREE.Vector3();

	let sky;
	let renderTarget;

	const sceneEnv = new THREE.Scene();

	const createsky = () => {
		sky = new Sky();
		sky.scale.setScalar(10000);
		scene.add(sky);
		const skyUniforms = sky.material.uniforms;
		skyUniforms['turbidity'].value = 10;
		skyUniforms['rayleigh'].value = 2;
		skyUniforms['mieCoefficient'].value = 0.005;
		skyUniforms['mieDirectionalG'].value = 0.8;
	};

	const createsun = () => {
		const parameters = {
			elevation: 2,
			azimuth: 180,
		};
		const phi = THREE.MathUtils.degToRad(90 - parameters.elevation);
		const theta = THREE.MathUtils.degToRad(parameters.azimuth);

		sun.setFromSphericalCoords(1, phi, theta);

		sky.material.uniforms['sunPosition'].value.copy(sun);


		waters.forEach((water) => {
			water.water.material.uniforms['sunDirection'].value.copy(sun).normalize();
		});

		if (renderTarget !== undefined) renderTarget.dispose();

		sceneEnv.add(sky);
		renderTarget = pmremGenerator.fromScene(sceneEnv);
		scene.add(sky);

		scene.environment = renderTarget.texture;
	};

	const createwater = (waterobject: waterobject) => {
		const waterGeometry = new THREE.PlaneGeometry(waterobject.width, waterobject.height);

		const water = new Water(waterGeometry, {
			textureWidth: 512,
			textureHeight: 512,
			waterNormals: new THREE.TextureLoader().load('/textures/water/waternormals.jpg', (texture) => {
				texture.wrapS = texture.wrapT = THREE.RepeatWrapping;
			}),
			sunDirection: new THREE.Vector3(),
			sunColor: 0xffffff,
			waterColor: 0x001e0f,
			distortionScale: 3.7,
			fog: scene.fog !== undefined,
		});
		water.rotation.x = waterobject.rx;
		water.rotation.y = waterobject.ry;
		water.rotation.z = waterobject.rz;

		water.position.y = waterobject.y;
		water.position.x = waterobject.x;
		water.position.z = waterobject.z;
		return water;
	};

	const createcubes = (cubeobject: cubeobject) => {
		const geometry = new THREE.BoxGeometry(cubeobject.width, cubeobject.height, cubeobject.depth);
		const material = new THREE.MeshBasicMaterial({ color: cubeobject.color });
		const mesh = new THREE.Mesh(geometry, material);
		mesh.position.set(cubeobject.x, cubeobject.y, cubeobject.z);
		const cube = new THREE.Mesh(geometry, material);
		cube.position.set(cubeobject.x, cubeobject.y, cubeobject.z);
		cube.rotateY(cubeobject.r);

		return cube;
	};

	const creat2dcssobject = (x: csstextobject) => {
		const htmlElement = document.createElement('div');
		htmlElement.style.width = '120px';
		htmlElement.style.height = '40px';
		htmlElement.style.background = 'rgba(255, 0, 255, 0.8)';
		htmlElement.style.color = 'black'; // 设置文字颜色以避免与背景颜色冲突
		htmlElement.style.fontSize = '32px'; // 调整字体大小
		htmlElement.style.textAlign = 'center';
		htmlElement.style.lineHeight = '40px'; // 垂直居中
		htmlElement.style.pointerEvents = 'auto'; //
		htmlElement.style.opacity = '0.5';
		htmlElement.style.display = 'block';
		htmlElement.innerText = x.text;
		htmlElement.bizId = x.bizId;
		htmlElement.addEventListener('click', (event) => {
			event.stopPropagation();
			if (state.panelvisable === false) {
				state.panelvisable = true;
				state.bizId = event.target['bizId'];
				devpanel.value.style.left = event.clientX - 220 + 'px';
				devpanel.value.style.top = event.clientY - 85 - devpanel.value.getBoundingClientRect().height + 'px';
			}
		});
		htmlElement.addEventListener('mouseover', (event) => {
			event.stopPropagation();
		});
		htmlElement.addEventListener('mouseout', (event) => {
			event.stopPropagation();
			// if (state.panelvisable === true) {
			//     console.log(event)
			//     state.panelvisable = false;

			// }
		});
		const cssObject = new CSS3DObject(htmlElement);
		cssObject.position.set(x.x, x.y, x.z);
		cssObject.scale.set(0.05, 0.1, 0.1);
		cssObject.rotateY(x.r);
		return cssObject;
	};

	const createlabel = (to: csstextobject) => {
		const canvas = document.createElement('canvas');
		const ctx = canvas.getContext('2d');
		canvas.width = 360;
		canvas.height = 128;
		ctx.fillStyle = 'rgba(0, 0, 0, 0.7)';
		ctx.fillRect(0, 0, canvas.width, canvas.height);
		ctx.fillStyle = 'white';
		ctx.font = '24px Arial';
		ctx.textAlign = 'center';
		ctx.textBaseline = 'middle';
		ctx.fillText(to.text, canvas.width / 2, canvas.height / 2);
		const texture = new THREE.CanvasTexture(canvas);
		const material = new THREE.SpriteMaterial({ map: texture });
		const sprite = new THREE.Sprite(material);
		sprite.position.copy(new THREE.Vector3(to.x, to.y, to.z));
		sprite.scale.set(3, 1.5, 1);
		return sprite;
	};

	const clock = new THREE.Clock();
	const loader = new GLTFLoader();
	const dracoloader = new DRACOLoader();
	const camera = new THREE.PerspectiveCamera(75, W / H, 0.1, 1000);
	const scene = new THREE.Scene();
	const renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true });

	const axesHelper = new THREE.AxesHelper(200);

	const textures = [
		new THREE.TextureLoader().load('/textures/light/disturb.jpg'),
		new THREE.TextureLoader().load('/textures/light/colors.png'),
		new THREE.TextureLoader().load('/textures/light/uv_grid_opengl.jpg'),
	];

	textures.forEach((texture) => {
		texture.minFilter = THREE.LinearFilter;
		texture.magFilter = THREE.LinearFilter;
		texture.generateMipmaps = false;
		texture.colorSpace = THREE.SRGBColorSpace;
	});

	const spotLight = new THREE.SpotLight(0xffffff, 100);
	spotLight.position.set(100.5, 100, 2.5);
	spotLight.angle = Math.PI / 6;
	spotLight.penumbra = 1;
	spotLight.decay = 2;
	spotLight.distance = 0;
	spotLight.map = textures[0];

	spotLight.castShadow = true;
	spotLight.shadow.mapSize.width = 1024;
	spotLight.shadow.mapSize.height = 1024;
	spotLight.shadow.camera.near = 1;
	spotLight.shadow.camera.far = 10;
	spotLight.shadow.focus = 1;
	spotLight.shadow.bias = -0.003;
	scene.add(spotLight);
	// const lightHelper = new THREE.SpotLightHelper(spotLight);
	// scene.add(lightHelper);
	renderer.setClearColor(0x999999);
	renderer.setSize(W, H);
	const cssRenderer = new CSS3DRenderer();
	cssRenderer.setSize(W, H);
	cssRenderer.domElement.style.position = 'absolute';
	cssRenderer.domElement.style.top = 0;
	cssRenderer.domElement.style.pointerEvents = 'none';
	webgl.value.appendChild(cssRenderer.domElement);
	webgl.value.appendChild(renderer.domElement);
	const pmremGenerator = new THREE.PMREMGenerator(renderer);
	const controls = new OrbitControls(camera, renderer.domElement);
	controls.enableDamping = true;
	window.addEventListener('mousemove', (event) => {
        //射线发射的位置和画布的偏移
		mouse.x = (event.clientX / (W + 440)) * 2 - 1;
		mouse.y = -(event.clientY / (H + 190)) * 2 + 1;
	});
	window.addEventListener('click', (x) => {
		raycaster.setFromCamera(mouse, camera);
		const intersects = raycaster.intersectObjects(labels);

		if (intersects && intersects.length > 0) {
			const clickedLabel = intersects[0].object;
		}
	});
	csstextobjects.forEach((x) => {
		scene.add(creat2dcssobject(x));
	});

	spriteobjects.forEach((x) => {
		var obj = createlabel(x);
		scene.add(obj);
		labels.push(obj);
	});
	//#ab4ae7
	waters.forEach((x) => {
		var water = createwater(x);
		x.water = water;
		scene.add(water);
	});

	cubeobjects.forEach((x) => {
		var cube = createcubes(x);
		x.cubu = cube;

		scene.add(cube);
	});

	createsky();

	createsun();
	scene.add(axesHelper);

	const texture = new THREE.TextureLoader().load('/imgs/mark/pin-blue.png');
	const spriteMaterial = new THREE.SpriteMaterial({
		map: texture,
		transparent: true, //Sprite //设置精灵纹理贴图
	});
	const sprite = new THREE.Sprite(spriteMaterial);
	scene.add(sprite);
	sprite.position.set(1, 8, 1);
	camera.position.set(20, 20, 20);
	dracoloader.setDecoderPath('./draco/gltf/');
	loader.setDRACOLoader(dracoloader);
	const mixer = new THREE.AnimationMixer();
	loader.load('/models/gltf/LittlestTokyo.glb', (gltf: any) => {
		const model = gltf.scene;
		model.position.set(0, 20, 0);
		model.scale.set(0.1, 0.1, 0.1);
		scene.background = new THREE.Color(0xbfe3dd);
		scene.environment = pmremGenerator.fromScene(new RoomEnvironment(), 0.04).texture;
		mixer.model = model;
		scene.add(model);

		//	mixer.clipAction(gltf.animations[0]).play();
		animate();
	});

	const animate = () => {
		requestAnimationFrame(animate);
		const delta = clock.getDelta();

		mixer.update(delta);
		raycaster.setFromCamera(mouse, camera);
		const intersects = raycaster.intersectObjects(labels);
		labels.forEach((label) => (label.material.opacity = 0.7));
		intersects.forEach((c) => (c.object.material.opacity = 1.0));

		controls.update();
		// Rotate the object

		move();
		waterprocess();
		renderer.render(scene, camera);
		cssRenderer.render(scene, camera);
	};

	const waterprocess = () => {
		waters.forEach((x) => {
			x.water.material.uniforms['time'].value += 1.0 / 360.0;
		});
	};

	const move = () => {
		cubeobjects.forEach((x: cubeobject) => {
			if (x.currentacton === 'up' && x.maxheight > x.cubu.position.y) {
				x.cubu.position.y += x.step;
			}

			if (x.currentacton === 'down' && x.minheight < x.cubu.position.y) {
				x.cubu.position.y -= x.step;
			}
		});
	};
});
</script>
<style scoped lang="scss">
.three-container {
	position: relative;
	width: 100%;
	height: 100vh;
	overflow: hidden;
}

.dev-panel {
	position: absolute;
	width: 600px;
	background-color: transparent;
}

.dev-card {
	max-width: 480px;
	background: #af4feb;
	opacity: 0.8;
	color: #020202;
}
</style>
